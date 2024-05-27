using DatabaseInterfacing.Domain.DTOs;
namespace Application.LogicInterfaces;

    public interface IAlertNotificationLogic
    {
        Task CreateAlertNotificationAsync(AlertNotificationDto dto);
        Task<IEnumerable<AlertNotificationDto>> GetAlertNotificationsAsync();
        Task UpdateAlertNotificationAsync(AlertNotificationDto dto);
        Task DeleteAlertNotificationAsync(int id);
    }
