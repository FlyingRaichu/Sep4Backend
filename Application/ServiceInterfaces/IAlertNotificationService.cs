namespace Application.ServiceInterfaces;

public interface IAlertNotificationService
{
        Task CheckAndTriggerAlertsAsync(string parameterType, float? value);
}