using DatabaseInterfacing.Domain.DTOs;

namespace Application.ServiceInterfaces;

public interface IAlertNotificationService
{
        Task CheckAndTriggerAlertsAsync(string parameterType, float? value);
        Task CreateAlertNotificationAsync(AlertNotificationDto alertNotificationDto);
        Task<IEnumerable<AlertNotificationDto>> GetAllAlertNotificationsAsync();
        Task UpdateAlertNotificationAsync(AlertNotificationDto alertNotificationDto);
        Task DeleteAlertNotificationAsync(int id);
}