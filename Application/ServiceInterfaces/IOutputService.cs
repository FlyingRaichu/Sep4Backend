using DatabaseInterfacing.Domain.DTOs;

namespace Application.ServiceInterfaces;

public interface IOutputService
{
    Task<MonitoringResultDto> AlterPumpAsync(string requestType, int valueInPercent);
}