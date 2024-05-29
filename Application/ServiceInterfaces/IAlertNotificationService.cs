using DatabaseInterfacing.Domain.DTOs;

namespace Application.ServiceInterfaces;

public interface IAlertNotificationService
{
        Task<IEnumerable<AlertNotificationDto>> GetAllAsync();
        Task UpdateAlertNotificationAsync(int id, AlertNotificationDto updateDto);
        Task CheckAndTriggerAlertsAsync(string parameterType, double? reading);

}