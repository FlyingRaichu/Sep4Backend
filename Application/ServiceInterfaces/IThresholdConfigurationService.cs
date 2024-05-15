using DatabaseInterfacing.Domain.DTOs;

namespace Application.ServiceInterfaces;

public interface IThresholdConfigurationService
{
    Task<ThresholdConfigurationDto> GetConfigurationAsync();
    Task UpdateConfigurationAsync(ThresholdDto dto); 
}