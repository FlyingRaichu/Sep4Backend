using DatabaseInterfacing.Domain.DTOs;
using DatabaseInterfacing.Domain.EntityFramework;

namespace Application.LogicInterfaces;

public interface IPlantDataLogic
{
    Task<IEnumerable<PlantData>> GetAsync(SearchPlantDataDto searchDto);
    Task<DisplayPlantWaterFlowDto> CheckWaterFlowAsync();
    Task<DisplayPlantTemperatureDto?> CheckTemperatureAsync(int id);
    Task<DisplayPlantPhDto> GetPhLevelAsync();
    Task<DisplayPlantECDto?> CheckECAsync(int id);
}