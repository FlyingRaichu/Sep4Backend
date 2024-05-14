using System.Text.Json;
using Application.LogicInterfaces;
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

    public PlantDataLogic(IConnectionController connectionController)
    {
        _connectionController = connectionController;
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

        if (searchDto.WaterEC != null)
        {
            query = query.Where(plant => plant.WaterEC.Equals(searchDto.WaterEC));
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

    var status = plantData?.Readings.FirstOrDefault()?.WaterTemperature switch
    {
        //The thresholds are placeholders, we need to figure out how to feed new placeholders up in this
        >= 50 and <= 75 => "Warn",
        > 75 => "Dang",
        _ => "Norm"
    };

    Console.WriteLine($"Plant water temp is: {plantData!.Readings.FirstOrDefault()?.WaterTemperature}");
    return new DisplayPlantTemperatureDto(plantData!.Readings.FirstOrDefault()?.WaterTemperature, status);
}
    
    public async Task<PlantPhDto> CheckPhLevelAsync(int id)
    {
        IConnectionController controller = new ConnectionController();
        string response = await controller.SendRequestToArduinoAsync("PARAMS");

        PlantData? plant = JsonSerializer.Deserialize<PlantData>(response);
        if (plant == null)
        {
            throw new Exception("Plant does not exist");
        }

        PlantPhDto dto = new PlantPhDto()
        {
            PlantId = plant.Id,
            PhLevel = plant.PhLevel
        };

        if (dto.PhLevel >= 6.2 || dto.PhLevel <= 6.8)
        {
            dto.IsOkay = true;
        }
        return dto;
    }

    public async Task<DisplayPlantECDto?> CheckECAsync(int id)
    {
        var jsonString = 
            await _connectionController.SendRequestToArduinoAsync(ApiParameters.DataRequest);
        
        
        var plantData = JsonSerializer.Deserialize<MonitoringResultDto>(jsonString,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        if (plantData == null) throw new Exception("Plant Data object is null or empty.");

        var status = plantData?.Readings.FirstOrDefault()?.WaterConductivity switch
        {
            //The thresholds are placeholders, we need to figure out how to feed new placeholders up in this
            >= 50 and <= 75 => "Warn",
            > 75 => "Dang",
            _ => "Norm"
        };

        Console.WriteLine($"Plant water temp is: {plantData!.Readings.FirstOrDefault()?.WaterConductivity}");
        return new DisplayPlantECDto(plantData!.Readings.FirstOrDefault()?.WaterConductivity, status);
    }

}