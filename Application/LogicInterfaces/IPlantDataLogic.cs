using DatabaseInterfacing.Domain.DTOs;
using DatabaseInterfacing.Domain.EntityFramework;

namespace Application.LogicInterfaces;

public interface IPlantDataLogic
{
    Task<IEnumerable<PlantData>> GetAsync(SearchPlantDataDto searchDto);
    Task<DisplayPlantTemperatureDto?> CheckTemperatureAsync(int id);
    Task<DisplayPlantECDto?> CheckECAsync(int id);
}