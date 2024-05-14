using System.Text.Json;
using Application.LogicInterfaces;
using DatabaseInterfacing;
using DatabaseInterfacing.Context;
using DatabaseInterfacing.Domain.DTOs;
using DatabaseInterfacing.Domain.EntityFramework;
using IoTInterfacing.Implementations;
using IoTInterfacing.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Application.Logic;

public class PlantDataLogic : IPlantDataLogic
{
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

        return await query.ToListAsync();
    }
    
    public async Task<PlantWaterFlowDto> CheckWaterFlowAsync(int id)
    {
        IConnectionController controller = new ConnectionController();
        string response = await controller.SendRequestToArduino("PARAMS");

        PlantData? plant = JsonSerializer.Deserialize<PlantData>(response);
        if (plant == null)
        {
            throw new Exception("Plant does not exist");
        }

        PlantWaterFlowDto dto = new PlantWaterFlowDto()
        {
            PlantId = plant.Id,
            WaterFlow = plant.WaterFlow
        };
        
        //test variables
        if (dto.WaterFlow >= 1.1 || dto.WaterFlow <= 2.2)
        {
            dto.IsOkay = true;
        }
        return dto;
    }
}