using System.Text.Json;
using Application.LogicInterfaces;
using Application.ServiceInterfaces;
using Application.Services;
using DatabaseInterfacing;
using DatabaseInterfacing.Context;
using DatabaseInterfacing.Domain.DTOs;
using DatabaseInterfacing.Domain.EntityFramework;
using IoTInterfacing.Interfaces;
using IoTInterfacing.Util;
using IoTInterfacing.Implementations;
using IoTInterfacing.Interfaces;
using Microsoft.EntityFrameworkCore;


namespace Application.Logic;

public class PlantDataLogic : IPlantDataLogic
{
    private readonly IConnectionController _connectionController;
    private readonly IThresholdConfigurationService _configurationService;
    private readonly IOutputService _outputService;

    private static string TEST = @"
        {
            ""name"" : ""monitoring_results"",
            ""readings"": [{
                ""waterConductivity"": 2622,
                ""waterTemperature"" : 23.5,
                ""waterPh"" : 7.1,
                ""waterFlow"" : 6.2,
                ""waterLevel"" : 12,
                ""airTemperature"" : 20,
                ""airHumidity"" : 50,
                ""airCo2"" : 400,
                ""lightLevel"" : 10000
            }]
        }";

    public PlantDataLogic(IConnectionController connectionController,
        IThresholdConfigurationService configurationService)
    {
        _connectionController = connectionController;
        _configurationService = configurationService;
        _outputService = new OutputService(_connectionController);
    }

    public async Task<IEnumerable<PlantData>> GetAsync(SearchPlantDataDto searchDto)
    {
        await using var dbContext = new PlantDbContext(DatabaseUtils.BuildConnectionOptions());

        var query = dbContext.PlantData.AsQueryable();

        if (!string.IsNullOrEmpty(searchDto.PlantName))
        {
            query = query.Where(
                plant => plant.PlantName.Contains(searchDto.PlantName));
        }

        if (searchDto.WaterTemperature != null)
        {
            query = query.Where(plant => plant.WaterTemperature.Equals(searchDto.WaterTemperature));
        }

        if (searchDto.PHLevel != null)
        {
            query = query.Where(plant => plant.PhLevel.Equals(searchDto.PHLevel));
        }

        if (searchDto.WaterFlow != null)
        {
            query = query.Where(plant => plant.WaterFlow.Equals(searchDto.WaterFlow));
        }

        if (searchDto.WaterEC != null)
        {
            query = query.Where(plant => plant.WaterEC.Equals(searchDto.WaterEC));
        }
        
        return await query.ToListAsync();
    }

    public async Task<MonitoringResultDto> GetAllDataAsync()
    {
        var jsonString =
            await _connectionController
                .SendRequestToArduinoAsync(ApiParameters.DataRequest);
        var plantData = JsonSerializer.Deserialize<MonitoringResultDto>(jsonString,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        if (plantData == null) throw new Exception("Plant Data object is null or empty.");
        await using var dbContext = new PlantDbContext(DatabaseUtils.BuildConnectionOptions());
        PlantData data = new PlantData()
        {
            PlantName = plantData.Name,
            PhLevel = (float)plantData.Readings.FirstOrDefault()?.WaterPhLevel!,
            WaterEC = (float)plantData.Readings.FirstOrDefault()?.WaterConductivity!,
            WaterTemperature = (float)plantData.Readings.FirstOrDefault()?.WaterTemperature!,
            WaterFlow = (float)plantData.Readings.FirstOrDefault()?.WaterFlow!,
            DateTime = DateTime.Now
        };
        await dbContext.PlantData.AddAsync(data);
        return plantData;
    }

    public async Task<DisplayPlantTemperatureDto?>
        CheckWaterTemperatureAsync()
    {
        var jsonString =
            await _connectionController
                .SendRequestToArduinoAsync(ApiParameters.DataRequest);

        //For testing remove // from line bellow 
        //var jsonString = TEST;

        //Deserialize the JSON string into a MonitoringResultDto object
        var plantData = JsonSerializer.Deserialize<MonitoringResultDto>(jsonString,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        if (plantData == null) throw new Exception("Plant Data object is null or empty.");

        //Calling the object created from the JSON config
        var configuration = await _configurationService.GetConfigurationAsync();

        var status = DetermineStatus("waterTemperature", plantData.Readings.FirstOrDefault()?.WaterTemperature,
            configuration);

        Console.WriteLine($"Plant water temp is: {plantData.Readings.FirstOrDefault()?.WaterTemperature}");
        return new DisplayPlantTemperatureDto(plantData.Readings.FirstOrDefault()?.WaterTemperature, status);
    }

    public async Task<DisplayPlantPhDto> GetPhLevelAsync()
    {
        // For Testing: Remove Comments from json, and comment out "string response = ..."

        var response = TEST;

        //string response = await _connectionController.SendRequestToArduinoAsync(ApiParameters.DataRequest);
        var plantData = JsonSerializer.Deserialize<MonitoringResultDto>(response, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        if (plantData == null) throw new Exception("Plant Data object is null or empty.");
        var configuration = await _configurationService.GetConfigurationAsync();
        var status = DetermineStatus("waterPh", plantData.Readings.FirstOrDefault()?.WaterPhLevel, configuration);
        return new DisplayPlantPhDto() { Status = status, PhLevel = plantData.Readings.FirstOrDefault()?.WaterPhLevel };
    }

    public async Task<DisplayPlantECDto?> CheckECAsync()
    {
        var jsonString =
            await _connectionController.SendRequestToArduinoAsync(ApiParameters.DataRequest);

        //var jsonString = TEST;

        var plantData = JsonSerializer.Deserialize<MonitoringResultDto>(jsonString,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        if (plantData == null) throw new Exception("Plant Data object is null or empty.");

        var configuration = await _configurationService.GetConfigurationAsync();
        var status = DetermineStatus("waterConductivity", plantData.Readings.FirstOrDefault()?.WaterConductivity,
            configuration);

        Console.WriteLine($"Plant water temp is: {plantData!.Readings.FirstOrDefault()?.WaterConductivity}");
        return new DisplayPlantECDto(plantData!.Readings.FirstOrDefault()?.WaterConductivity, status);
    }

    public async Task<DisplayPlantWaterFlowDto> CheckWaterFlowAsync()
    {
        var response = TEST;
        //var response = await _connectionController.SendRequestToArduinoAsync(ApiParameters.DataRequest);

        var plantData = JsonSerializer.Deserialize<MonitoringResultDto>(response,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        if (plantData == null) throw new Exception("Plant Data object is null or empty.");

        var configuration = await _configurationService.GetConfigurationAsync();
        var reading = plantData.Readings.First().WaterFlow ?? -900000;
        var status = DetermineStatus("waterFlow", reading, configuration);

        var dto = new DisplayPlantWaterFlowDto()
        {
            Status = status,
            WaterFlow = (float)plantData?.Readings?.FirstOrDefault()?.WaterFlow!
        };
        
        var waterFlowValues = configuration.Thresholds.FirstOrDefault(type => type.Type.Equals("waterFlow"))!;
        
        var perfectThreshold = (waterFlowValues.WarningMin + waterFlowValues.WarningMax) / 2;
        
        //TODO Test this
        //PID and output logic
        var pid = new PidService(0.1, 0.1, 0.1, perfectThreshold);

        await _outputService.AlterPumpAsync("waterFlowCorrection", pid.Compute(reading, 5));
        
        return dto;
    }

    public async Task<DisplayPlantWaterLevelDto> CheckWaterLevelAsync()
    {
        var response = TEST;
        // var response = await _connectionController.SendRequestToArduinoAsync(ApiParameters.DataRequest);

        var plantData = JsonSerializer.Deserialize<MonitoringResultDto>(response, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (plantData == null) throw new Exception("Plant Data object is null or empty");

        var configuration = await _configurationService.GetConfigurationAsync();
        var status = DetermineStatus("waterLevel", plantData.Readings.FirstOrDefault()?.WaterLevel, configuration);

        var dto = new DisplayPlantWaterLevelDto
        {
            Status = status,
            WaterLevelInMillimeters = (float)plantData?.Readings?.FirstOrDefault()?.WaterLevel!
        };

        return dto;
    }

    public async Task<DisplayAirTemperatureDto> CheckAirTemperatureAsync()
    {
        var response = TEST;
        // var response = await _connectionController.SendRequestToArduinoAsync(ApiParameters.DataRequest);
        var plantData = JsonSerializer.Deserialize<MonitoringResultDto>(response, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (plantData == null) throw new Exception("Plant Data object is null or empty.");

        var configuration = await _configurationService.GetConfigurationAsync();
        var status = DetermineStatus("airTemperature", plantData.Readings?.FirstOrDefault()?.AirTemperature,
            configuration);

        var dto = new DisplayAirTemperatureDto()
        {
            Status = status,
            AirTemperatureInC = (float)plantData.Readings?.FirstOrDefault()?.AirTemperature!
        };

        return dto;
    }

    public async Task<DisplayAirHumidityDto> CheckAirHumidityAsync()
    {
        var response = TEST;
        // var response = await _connectionController.SendRequestToArduinoAsync(ApiParameters.DataRequest);
        var plantData = JsonSerializer.Deserialize<MonitoringResultDto>(response, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        
        if (plantData == null) throw new Exception("Plant Data object is null or empty.");

        var configuration = await _configurationService.GetConfigurationAsync();
        var status = DetermineStatus("airHumidity", plantData.Readings?.FirstOrDefault()?.AirHumidity, configuration);

        var dto = new DisplayAirHumidityDto()
        {
            Status = status,
            AirHumidityPercentage = (float)plantData.Readings?.FirstOrDefault()?.AirHumidity!
        };
        
        return dto;
    }
    
    public async Task<DisplayAirCO2Dto> CheckAirCO2Async()
    {
        var response = TEST;
        // var response = await _connectionController.SendRequestToArduinoAsync(ApiParameters.DataRequest);

        var plantData = JsonSerializer.Deserialize<MonitoringResultDto>(response, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (plantData == null) throw new Exception("Plant Data object is null or empty.");

        var configuration = await _configurationService.GetConfigurationAsync();
        var status = DetermineStatus("airCo2", plantData.Readings?.FirstOrDefault()?.AirCO2, configuration);

        var dto = new DisplayAirCO2Dto()
        {
            Status = status,
            AirCO2 = plantData.Readings?.FirstOrDefault()?.AirCO2!
        };

        return dto;
    }
    
    public async Task<DisplayVPDLevelDto> CheckVPDAsync()
    {
        var response = TEST;
        // var response = await _connectionController.SendRequestToArduinoAsync(ApiParameters.DataRequest);

        var plantData = JsonSerializer.Deserialize<MonitoringResultDto>(response, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (plantData == null) throw new Exception("Plant Data object is null or empty.");

        var airTemperature = plantData.Readings?.FirstOrDefault()?.AirTemperature;
        var airHumidity = plantData.Readings?.FirstOrDefault()?.AirHumidity;

        if (airTemperature == null || airHumidity == null) throw new Exception("Insufficient data to calculate VPD.");

        var vpd = CalculateVPD(airTemperature.Value, airHumidity.Value);
        var configuration = await _configurationService.GetConfigurationAsync();
        var status = DetermineStatus("vpdLevel", vpd, configuration);

        return new DisplayVPDLevelDto()
        {
            Status = status,
            VPDLevel = vpd
        };
    }
    
    public async Task<DisplayDewPointDto> CheckDewPointAsync()
    {
        var response = TEST;
        // var response = await _connectionController.SendRequestToArduinoAsync(ApiParameters.DataRequest);

        var plantData = JsonSerializer.Deserialize<MonitoringResultDto>(response, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (plantData == null) throw new Exception("Plant Data object is null or empty.");

        var airTemperature = plantData.Readings?.FirstOrDefault()?.AirTemperature;
        var airHumidity = plantData.Readings?.FirstOrDefault()?.AirHumidity;

        if (airTemperature == null || airHumidity == null) throw new Exception("Insufficient data to calculate Dew Point.");

        var dewPoint = CalculateDewPoint(airTemperature.Value, airHumidity.Value);
        var configuration = await _configurationService.GetConfigurationAsync();
        var status = DetermineStatus("dewPoint", dewPoint, configuration);

        return new DisplayDewPointDto()
        {
            Status = status,
            DewPoint = dewPoint
        };
    }

    public async Task<DisplayLightLevelDto> CheckLightLevelAsync()
    {
        var response = TEST;
        var plantData = JsonSerializer.Deserialize<MonitoringResultDto>(response, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        if (plantData == null) throw new Exception("Plant Data object is null or empty.");

        var configuration = await _configurationService.GetConfigurationAsync();
        var status = DetermineStatus("lightLevel", plantData.Readings.FirstOrDefault()?.LightLevel, configuration);

        return new DisplayLightLevelDto(plantData.Readings.FirstOrDefault()?.LightLevel, status);
    }

    private static float CalculateVPD(float temperature, float humidity)
    {
        // Calculation for VPD
        float es = 0.6108f * (float)Math.Exp((17.27f * temperature) / (temperature + 237.3f));
        float ea = humidity / 100 * es;
        return es - ea;
    }
    
    private static float CalculateDewPoint(float temperature, float humidity)
    {
        // Calculation for Dew Point
        double a = 17.27;
        double b = 237.7;
        double alpha = ((a * temperature) / (b + temperature)) + Math.Log(humidity / 100.0);
        return (float)((b * alpha) / (a - alpha));
    }
    
    private static string DetermineStatus(string type, float? reading, ThresholdConfigurationDto config)
    {
        var threshold = config.Thresholds.FirstOrDefault(dto => dto.Type.Equals(type));
        if (threshold == null) throw new Exception("Threshold does not exist!");

        var isWarningRange =
            (reading <= threshold.WarningMin && reading > threshold.Min) ||
            (reading >= threshold.WarningMax && reading < threshold.Max);

        var isDangerRange = reading >= threshold.Max || reading <= threshold.Min;
        if (isDangerRange) return "Dang";

        return isWarningRange ? "Warn" : "Norm";
    }
}