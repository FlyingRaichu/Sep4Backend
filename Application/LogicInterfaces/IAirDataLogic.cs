using DatabaseInterfacing.Domain.DTOs;

namespace Application.LogicInterfaces;

public interface IAirDataLogic
{
    Task<DisplayAirTemperatureDto> CheckAirTemperatureAsync();
    Task<DisplayAirHumidityDto> CheckAirHumidityAsync();
    Task<DisplayAirCO2Dto> CheckAirCO2Async();
    Task<DisplayVPDLevelDto> CheckVPDAsync();
    Task<DisplayDewPointDto> CheckDewPointAsync();
    Task<DisplayLightLevelDto> CheckLightLevelAsync();
}