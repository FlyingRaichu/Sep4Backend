using DatabaseInterfacing.Domain.DTOs;
namespace Application.LogicInterfaces;

    public interface IAlertNotificationLogic
    {
        Task<AlertNotificationDto> GetAlertNotificationAsync(int id);
        Task UpdateAlertNotificationAsync(int id, bool isThresholdMinEnabled, bool isThresholdMaxEnabled);
        Task<IEnumerable<AlertNotificationDto>> GetAlertNotificationsAsync();
    }
