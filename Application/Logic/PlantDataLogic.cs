using System.Globalization;
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
    private readonly IAlertNotificationService _alertNotificationService;
    private readonly IOutputService _outputService;

    private static string TEST = @"
        {
        ""name"": ""monitoring_results"",
        ""readings"":     [{
                        ""waterConductivity"":    2622
                }, {
                        ""waterPh"":      6
                }, {
                        ""waterTemperature"":     15
                }, {
                        ""waterFlow"":    0
                }, {
                        ""waterLevel"":   16
                }, {
                        ""airTemperature"":       24
                }, {
                        ""airHumidity"":  48
                }, {
                        ""lightLevel"":   166
                }, {
                        ""airCo2"":       1250
                }]
}";

    public PlantDataLogic(IConnectionController connectionController,
        IThresholdConfigurationService configurationService,
        IAlertNotificationService alertNotificationService)
    {
        _connectionController = connectionController;
        _configurationService = configurationService;
        _alertNotificationService = alertNotificationService;
        _outputService = new OutputService(_connectionController);
    }

    public async Task<IEnumerable<PlantData>> GetAsync(SearchPlantDataDto searchDto)
    {
        await using var dbContext = new PlantDbContext(DatabaseUtils.BuildConnectionOptions());

        var query = dbContext.PlantData.AsQueryable();

        if (!string.IsNullOrEmpty(searchDto.PlantName))
            query = query.Where(plant => plant.PlantName.Contains(searchDto.PlantName));

        if (searchDto.WaterTemperature != null)
            query = query.Where(plant => plant.WaterTemperature.Equals(searchDto.WaterTemperature));

        if (searchDto.WaterPhLevel != null)
            query = query.Where(plant => plant.WaterPhLevel.Equals(searchDto.WaterPhLevel));

        if (searchDto.WaterFlow != null)
            query = query.Where(plant => plant.WaterFlow.Equals(searchDto.WaterFlow));

        if (searchDto.WaterLevel != null)
            query = query.Where(plant => plant.WaterLevel.Equals(searchDto.WaterLevel));

        if (searchDto.AirTemperature != null)
            query = query.Where(plant => plant.AirTemperature.Equals(searchDto.AirTemperature));

        if (searchDto.AirHumidity != null)
            query = query.Where(plant => plant.AirHumidity.Equals(searchDto.AirHumidity));

        if (searchDto.AirCO2 != null)
            query = query.Where(plant => plant.AirCO2.Equals(searchDto.AirCO2));

        if (searchDto.LightLevel != null)
            query = query.Where(plant => plant.LightLevel.Equals(searchDto.LightLevel));

        if (searchDto.DewPoint != null)
            query = query.Where(plant => plant.DewPoint.Equals(searchDto.DewPoint));

        if (searchDto.VpdLevel != null)
            query = query.Where(plant => plant.VpdLevel.Equals(searchDto.VpdLevel));

        if (searchDto.DateTime != null)
            query = query.Where(plant => plant.DateTime.Equals(searchDto.DateTime));

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
        var dewPoint = await CheckDewPointAsync();
        var vpdLevel = await CheckVPDAsync();
        var formattedDateTime = DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss");
        var data = new PlantData()
        {
            PlantName = plantData.Name,
            WaterPhLevel = (float)plantData.Readings.FirstOrDefault()?.Measurements["waterPh"].GetSingle()!,
            WaterConductivity = (float)plantData.Readings.FirstOrDefault()?.Measurements["waterConductivity"].GetSingle()!,
            WaterTemperature = (float)plantData.Readings.FirstOrDefault()?.Measurements["waterTemperature"].GetSingle()!,
            WaterFlow = (float)plantData.Readings.FirstOrDefault()?.Measurements["waterFlow"].GetSingle()!,
            WaterLevel = (float)plantData.Readings.FirstOrDefault()?.Measurements["waterLevel"].GetSingle()!,
            AirHumidity = (float)plantData.Readings.FirstOrDefault()?.Measurements["airHumidity"].GetSingle()!,
            AirCO2 = (float)plantData.Readings.FirstOrDefault()?.Measurements["airCo2"].GetSingle()!,
            AirTemperature = (float)plantData.Readings.FirstOrDefault()?.Measurements["airTemperature"].GetSingle()!,
            LightLevel = (float)plantData.Readings.FirstOrDefault()?.Measurements["lightLevel"].GetSingle()!,
            DewPoint = (float)dewPoint.DewPoint!,
            VpdLevel = (float)vpdLevel.VPDLevel!,
            
            DateTime = DateTime.ParseExact(formattedDateTime, "MM/dd/yyyy hh:mm:ss", CultureInfo.InvariantCulture)
        };
        await dbContext.PlantData.AddAsync(data);
        await dbContext.SaveChangesAsync();
        await CheckAndTriggerAlertsAsync(plantData);
        return plantData;
    }

    private async Task CheckAndTriggerAlertsAsync(MonitoringResultDto plantData)
    {
        var readings = plantData.Readings.FirstOrDefault();
        if (readings != null)
        {
            await _alertNotificationService.CheckAndTriggerAlertsAsync("water_temperature", readings.Measurements["waterTemperature"].GetSingle());
            await _alertNotificationService.CheckAndTriggerAlertsAsync("water_ph", readings.Measurements["waterPh"].GetSingle());
            await _alertNotificationService.CheckAndTriggerAlertsAsync("water_conductivity", readings.Measurements["waterConductivity"].GetSingle());
            await _alertNotificationService.CheckAndTriggerAlertsAsync("water_flow", readings.Measurements["waterFlow"].GetSingle());
        }
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

        var status = DetermineStatus("waterTemperature", plantData.Readings.FirstOrDefault()?.Measurements["waterTemperature"].GetSingle(),
            configuration);

        Console.WriteLine($"Plant water temp is: {plantData.Readings.FirstOrDefault()?.Measurements["waterTemperature"].GetSingle()}");
        return new DisplayPlantTemperatureDto(plantData.Readings.FirstOrDefault()?.Measurements["waterTemperature"].GetSingle(), status);
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
        var status = DetermineStatus("waterPh", plantData.Readings.FirstOrDefault()?.Measurements["waterPh"].GetSingle(), configuration);
        return new DisplayPlantPhDto() { Status = status, PhLevel = plantData.Readings.FirstOrDefault()?.Measurements["waterPh"].GetSingle() };
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
        var status = DetermineStatus("waterConductivity", plantData.Readings.FirstOrDefault()?.Measurements["waterConductivity"].GetSingle(),
            configuration);

        Console.WriteLine($"Plant water temp is: {plantData!.Readings.FirstOrDefault()?.Measurements["waterConductivity"]}");
        return new DisplayPlantECDto(plantData!.Readings.FirstOrDefault()?.Measurements["waterConductivity"].GetSingle(), status);
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
        var reading = plantData.Readings.First().Measurements["waterFlow"];
        var status = DetermineStatus("waterFlow", reading.GetSingle(), configuration);

        var dto = new DisplayPlantWaterFlowDto()
        {
            Status = status,
            WaterFlow = (float)plantData?.Readings?.FirstOrDefault()?.Measurements["waterFlow"].GetSingle()!
        };
        
        var waterFlowValues = configuration.Thresholds.FirstOrDefault(type => type.Type.Equals("waterFlow"))!;
        
        var perfectThreshold = (waterFlowValues.WarningMin + waterFlowValues.WarningMax) / 2;
        
        //TODO Test this
        //PID and output logic
        var pid = new PidService(0.5, 0.1, 0.1, perfectThreshold);

        await _outputService.AlterPumpAsync("waterFlowCorrection", pid.Compute(reading.GetSingle(), 5));
        
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
        var status = DetermineStatus("waterLevel", plantData.Readings.FirstOrDefault()?.Measurements["waterLevel"].GetSingle(), configuration);

        var dto = new DisplayPlantWaterLevelDto
        {
            Status = status,
            WaterLevelInMillimeters = (float)plantData?.Readings?.FirstOrDefault()?.Measurements["waterLevel"].GetSingle()!
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
        var status = DetermineStatus("airTemperature", plantData.Readings?.FirstOrDefault()?.Measurements["airTemperature"].GetSingle(),
            configuration);

        var dto = new DisplayAirTemperatureDto()
        {
            Status = status,
            AirTemperatureInC = (float)plantData.Readings?.FirstOrDefault()?.Measurements["airTemperature"].GetSingle()!
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
        var status = DetermineStatus("airHumidity", plantData.Readings?.FirstOrDefault()?.Measurements["airHumidity"].GetSingle(), configuration);

        var dto = new DisplayAirHumidityDto()
        {
            Status = status,
            AirHumidityPercentage = (float)plantData.Readings?.FirstOrDefault()?.Measurements["airHumidity"].GetSingle()!
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
        var status = DetermineStatus("airCo2", plantData.Readings?.FirstOrDefault()?.Measurements["airCo2"].GetSingle(), configuration);

        var dto = new DisplayAirCO2Dto()
        {
            Status = status,
            AirCO2 = plantData.Readings?.FirstOrDefault()?.Measurements["airCo2"].GetSingle()!
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

        var airTemperature = plantData.Readings?.FirstOrDefault()?.Measurements["airTemperature"];
        var airHumidity = plantData.Readings?.FirstOrDefault()?.Measurements["airHumidity"];

        if (airTemperature == null || airHumidity == null) throw new Exception("Insufficient data to calculate VPD.");

        var vpd = CalculateVPD(airTemperature.Value.GetSingle(), airHumidity.Value.GetSingle());
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

        var airTemperature = plantData.Readings?.FirstOrDefault()?.Measurements["airTemperature"];
        var airHumidity = plantData.Readings?.FirstOrDefault()?.Measurements["airHumidity"];

        if (airTemperature == null || airHumidity == null)
            throw new Exception("Insufficient data to calculate Dew Point.");

        var dewPoint = CalculateDewPoint(airTemperature.Value.GetSingle(), airHumidity.Value.GetSingle());
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
        var status = DetermineStatus("lightLevel", plantData.Readings.FirstOrDefault()?.Measurements["lightLevel"].GetSingle(), configuration);

        return new DisplayLightLevelDto(plantData.Readings.FirstOrDefault()?.Measurements["lightLevel"].GetSingle(), status);
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

    // public async Task<ICollection<MeasurementDto>> GetAllMeasurementsAsync()
    // {
    //     await using var _context = new PlantDbContext(DatabaseUtils.BuildConnectionOptions());
    //     var measurements = await _context.Measurements
    //         .Select(m => new MeasurementDto()
    //         {
    //             Id = m.Id,
    //             Time = m.Time,
    //             WaterTemperature = m.WaterTemperature,
    //             WaterPH = m.WaterPH,
    //             ElectricConductivity = m.ElectricConductivity,
    //             FlowRate = m.FlowRate,
    //             WaterLevel = m.WaterLevel,
    //             AirTemperature = m.AirTemperature,
    //             AirHumidity = m.AirHumidity,
    //             CO2 = m.CO2,
    //             VPD = m.VPD,
    //             DewPoint = m.DewPoint,
    //             LightLevels = m.LightLevels
    //         })
    //         .ToListAsync();
    //     return measurements;
    // }
}