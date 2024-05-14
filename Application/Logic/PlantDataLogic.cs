using System.Text.Json;
using Application.LogicInterfaces;
using Application.ServiceInterfaces;
using DatabaseInterfacing;
using DatabaseInterfacing.Context;
using DatabaseInterfacing.Domain.DTOs;
using DatabaseInterfacing.Domain.EntityFramework;
using IoTInterfacing.Interfaces;
using IoTInterfacing.Util;
using Microsoft.EntityFrameworkCore;


namespace Application.Logic;

public class PlantDataLogic : IPlantDataLogic
{
    private readonly IConnectionController _connectionController;
    private readonly IThresholdConfigurationService _configurationService;

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

        return await query.ToListAsync();
    }

    public async Task<DisplayPlantTemperatureDto?> CheckTemperatureAsync(int id) //Not sure Ids are supposed to be here
    {
        var jsonString =
            await _connectionController
                .SendRequestToArduinoAsync(ApiParameters.DataRequest);
//     var jsonString = @"
// {
//     ""name"" : ""monitoring_results"",
//     ""readings"": [{
//         ""water_conductivity"": 2622,
//         ""water_temperature"" : 23.5
//     }]
// }";


        //Deserialize the JSON string into a MonitoringResultDto object
        var plantData = JsonSerializer.Deserialize<MonitoringResultDto>(jsonString,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        if (plantData == null) throw new Exception("Plant Data object is null or empty.");

        //Calling the object created from the JSON config
        var configuration = await _configurationService.GetConfigurationAsync();

        var status = DetermineTemperatureStatus(plantData.Readings.FirstOrDefault()?.WaterTemperature, configuration);

        Console.WriteLine($"Plant water temp is: {plantData!.Readings.FirstOrDefault()?.WaterTemperature}");
        return new DisplayPlantTemperatureDto(plantData!.Readings.FirstOrDefault()?.WaterTemperature, status);
    }


    private static string DetermineTemperatureStatus(float? waterTemperature, ThresholdConfigurationDto config)
    {
        var isWarningRange =
            (waterTemperature <= config.WarningTemperatureMin && waterTemperature > config.MinTemperature) ||
            (waterTemperature >= config.WarningTemperatureMax && waterTemperature < config.MaxTemperature);
        var isDangerRange = waterTemperature >= config.MaxTemperature || waterTemperature <= config.MinTemperature;

        if (isDangerRange)
        {
            return "Dang";
        }

        return isWarningRange ? "Warn" : "Norm";
    }
}