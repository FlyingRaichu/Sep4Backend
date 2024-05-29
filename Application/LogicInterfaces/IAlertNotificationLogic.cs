using DatabaseInterfacing.Domain.DTOs;
namespace Application.LogicInterfaces;

    public interface IAlertNotificationLogic
    {
        Task<AlertNotificationDto> GetAlertNotificationAsync(int id);
        Task UpdateAlertNotificationAsync(int id, double thresholdMin, double thresholdMax);
        Task CheckAndTriggerAlertsAsync(string parameterType, double? reading);
    }
