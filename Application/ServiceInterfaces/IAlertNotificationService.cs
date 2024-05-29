using DatabaseInterfacing.Domain.DTOs;

namespace Application.ServiceInterfaces;

public interface IAlertNotificationService
{
        Task CheckAndTriggerAlertsAsync(string parameterType, double? reading);

}