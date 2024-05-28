using DatabaseInterfacing.Domain.DTOs;
using DatabaseInterfacing.Domain.EntityFramework;

namespace Application.LogicInterfaces;

public interface IPlantDataLogic
{
    bool WaterFlowCorrectionEnabled { get; set; }
    Task<IEnumerable<PlantData>> GetAsync(SearchPlantDataDto searchDto);
    Task<MonitoringResultDto> GetAllDataAsync();
    Task<DisplayPlantWaterFlowDto> CheckWaterFlowAsync();
    Task<DisplayPlantTemperatureDto?> CheckWaterTemperatureAsync();
    Task<DisplayPlantPhDto> GetPhLevelAsync();
    Task<DisplayPlantECDto?> CheckECAsync();
    Task<DisplayPlantWaterLevelDto> CheckWaterLevelAsync();
    Task<DisplayAirTemperatureDto> CheckAirTemperatureAsync();
    Task<DisplayAirHumidityDto> CheckAirHumidityAsync();
    Task<DisplayAirCO2Dto> CheckAirCO2Async();
    Task<DisplayVPDLevelDto> CheckVPDAsync();
    Task<DisplayDewPointDto> CheckDewPointAsync();
    Task<DisplayLightLevelDto> CheckLightLevelAsync();
    // Task<ICollection<MeasurementDto>> GetAllMeasurementsAsync();
}