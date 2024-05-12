using System.Text.Json;
using Application.LogicInterfaces;
using DatabaseInterfacing;
using DatabaseInterfacing.Context;
using DatabaseInterfacing.Domain.DTOs;
using DatabaseInterfacing.Domain.EntityFramework;
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
                .SendRequestToArduinoAsync("PLACEHOLDER"); //TODO change this to the actual call's parameters

        //Deserialize the JSON string into a PlantData object
        var plantData = JsonSerializer.Deserialize<PlantData>(jsonString,
            new JsonSerializerOptions //NOTE: This might not be this simple considering we
                //have to manually deserialize a bunch of fields based on what IoT are going to be sending
                {
                    PropertyNameCaseInsensitive = true
                });
        if (plantData == null) throw new Exception("Plant Data object is null or empty.");

        var status = plantData?.WaterTemperature switch
        {
            //The thresholds are placeholders, we need to figure out how to feed new placeholders up in this
            >= 50 and <= 75 => "Warn",
            > 75 => "Dang",
            _ => "Norm"
        };

        return new DisplayPlantTemperatureDto(plantData!.WaterTemperature, status);
    }

    public async Task<DisplayWaterECStatusDto?> CheckWaterECAsync(int id)
    {
        var jsonString =
            await _connectionController
                .SendRequestToArduinoAsync("PLACEHOLDER"); //TODO change this to the actual call's parameters

        //Deserialize the JSON string into a PlantData object
        var plantData = JsonSerializer.Deserialize<PlantData>(jsonString,
            new JsonSerializerOptions 
                {
                    PropertyNameCaseInsensitive = true
                });
        if (plantData == null) throw new Exception("Plant Data object is null or empty.");

        var status = plantData?.WaterEC switch
        {
            // Similar to temperature, define thresholds for EC levels
            // Placeholder thresholds
            >= 100 and <= 200 => "Norm",
            > 200 => "Dang",
            _ => "Warn"
        };

        return new DisplayWaterECStatusDto(plantData!.WaterEC, status);
    }

}