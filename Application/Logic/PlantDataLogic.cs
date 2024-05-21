using System.Text.Json;
using Application.LogicInterfaces;
using Application.ServiceInterfaces;
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

    private static string TEST = @"
        {
            ""name"" : ""monitoring_results"",
            ""readings"": [{
                ""water_conductivity"": 2622,
                ""water_temperature"" : 23.5,
                ""water_ph"" : 7.1,
                ""water_flow"" : 6.2,
                ""water_level"" : 12,
                ""air_temperature"" : 20,
                ""air_humidity"" : 50,
                ""air_co2"" : 400,
                ""light_level"" : 10000
            }]
        }";

    public PlantDataLogic(IConnectionController connectionController,
        IThresholdConfigurationService configurationService)
    {
        _connectionController = connectionController;
        _configurationService = configurationService;
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

    public async Task<DisplayPlantTemperatureDto?>
        CheckWaterTemperatureAsync(int id) //Not sure Ids are supposed to be here
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

        var status = DetermineStatus("water_temperature", plantData.Readings.FirstOrDefault()?.WaterTemperature,
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
        var status = DetermineStatus("water_ph", plantData.Readings.FirstOrDefault()?.WaterPhLevel, configuration);
        return new DisplayPlantPhDto() { Status = status, PhLevel = plantData.Readings.FirstOrDefault()?.WaterPhLevel };
    }

    public async Task<DisplayPlantECDto?> CheckECAsync(int id)
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
        var status = DetermineStatus("water_conductivity", plantData.Readings.FirstOrDefault()?.WaterConductivity,
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
        var status = DetermineStatus("water_flow", plantData.Readings.FirstOrDefault()?.WaterFlow, configuration);

        DisplayPlantWaterFlowDto dto = new DisplayPlantWaterFlowDto()
        {
            Status = status,
            WaterFlow = (float)plantData?.Readings?.FirstOrDefault()?.WaterFlow!
        };
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
        var status = DetermineStatus("water_level", plantData.Readings.FirstOrDefault()?.WaterLevel, configuration);

        var dto = new DisplayPlantWaterLevelDto()
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
        var status = DetermineStatus("air_temperature", plantData.Readings?.FirstOrDefault()?.AirTemperature,
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
        var status = DetermineStatus("air_humidity", plantData.Readings?.FirstOrDefault()?.AirHumidity, configuration);

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
        var status = DetermineStatus("air_co2", plantData.Readings?.FirstOrDefault()?.AirCO2, configuration);

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
        var status = DetermineStatus("vpd_level", vpd, configuration);

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
        var status = DetermineStatus("dew_point", dewPoint, configuration);

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
        var status = DetermineStatus("light_level", plantData.Readings.FirstOrDefault()?.LightLevel, configuration);

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