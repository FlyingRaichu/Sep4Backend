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
    private readonly IWaterDataLogic _waterDataLogic;
    private readonly IAirDataLogic _airDataLogic;

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
        IAlertNotificationService alertNotificationService, IWaterDataLogic waterDataLogic, IAirDataLogic airDataLogic)
    {
        _connectionController = connectionController;
        _configurationService = configurationService;
        _alertNotificationService = alertNotificationService;
        _waterDataLogic = waterDataLogic;
        _airDataLogic = airDataLogic;
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
        
        if (searchDto.WaterConductivity != null)
            query = query.Where(plant => plant.AirTemperature.Equals(searchDto.WaterConductivity));

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

    public async Task<bool> ToggleWaterFlowCorrectionAsync()
    {
       return await _waterDataLogic.ToggleWaterFlowCorrectionAsync();
    }

    public async Task<DisplayPlantTemperatureDto?> CheckWaterTemperatureAsync()
    {
        return await _waterDataLogic.CheckWaterTemperatureAsync();
    }

    public async Task<DisplayPlantPhDto> GetPhLevelAsync()
    {
        return await _waterDataLogic.GetPhLevelAsync();
    }

    public async Task<DisplayPlantECDto?> CheckECAsync()
    {
        return await _waterDataLogic.CheckECAsync();
    }

    public async Task<DisplayPlantWaterFlowDto> CheckWaterFlowAsync()
    {
        return await _waterDataLogic.CheckWaterFlowAsync();
    }

    public async Task<DisplayPlantWaterLevelDto> CheckWaterLevelAsync()
    {
        return await _waterDataLogic.CheckWaterLevelAsync();
    }

    public async Task<DisplayAirTemperatureDto> CheckAirTemperatureAsync()
    {
        return await _airDataLogic.CheckAirTemperatureAsync();
    }

    public async Task<DisplayAirHumidityDto> CheckAirHumidityAsync()
    {
        return await _airDataLogic.CheckAirHumidityAsync();
    }

    public async Task<DisplayAirCO2Dto> CheckAirCO2Async()
    {
        return await _airDataLogic.CheckAirCO2Async();
    }

    public async Task<DisplayVPDLevelDto> CheckVPDAsync()
    {
        return await _airDataLogic.CheckVPDAsync();
    }

    public async Task<DisplayDewPointDto> CheckDewPointAsync()
    {
        return await _airDataLogic.CheckDewPointAsync();
    }

    public async Task<DisplayLightLevelDto> CheckLightLevelAsync()
    {
        return await _airDataLogic.CheckLightLevelAsync();
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