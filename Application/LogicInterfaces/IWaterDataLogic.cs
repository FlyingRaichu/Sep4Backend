using DatabaseInterfacing.Domain.DTOs;
using DatabaseInterfacing.Domain.EntityFramework;

namespace Application.LogicInterfaces;

public interface IWaterDataLogic
{
    Task<DisplayPlantTemperatureDto?> CheckWaterTemperatureAsync();
    Task<DisplayPlantPhDto> GetPhLevelAsync();
    Task<DisplayPlantECDto?> CheckECAsync();
    Task<DisplayPlantWaterFlowDto> CheckWaterFlowAsync();
    Task<DisplayPlantWaterLevelDto> CheckWaterLevelAsync();
    Task<bool> ToggleWaterFlowCorrectionAsync();
}