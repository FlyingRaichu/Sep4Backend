using System.Text.Json;
using Application.LogicInterfaces;
using Application.ServiceInterfaces;
using DatabaseInterfacing;
using DatabaseInterfacing.Context;
using DatabaseInterfacing.Domain.DTOs;
using DatabaseInterfacing.Domain.EntityFramework;
using Microsoft.EntityFrameworkCore;

namespace Application.Services
{
    public class AlertNotificationService : IAlertNotificationService
    {
        private readonly IAlertNotificationLogic _alertNotificationLogic;
        private readonly IEmailService _emailService;

        public AlertNotificationService(IAlertNotificationLogic alertNotificationLogic, IEmailService emailService)
        {
            _alertNotificationLogic = alertNotificationLogic;
            _emailService = emailService;
        }

        public async Task CheckAndTriggerAlertsAsync(string parameterType, double? reading)
        {
            if (reading == null)
                return;

            var alerts = await _alertNotificationLogic.GetAlertNotificationsAsync();
            var relevantAlerts = alerts.Where(a => a.ParameterType == parameterType).ToList();

            foreach (var alert in relevantAlerts)
            {
                if ((alert.IsThresholdMinEnabled && reading <= alert.ThresholdMin) ||
                    (alert.IsThresholdMaxEnabled && reading >= alert.ThresholdMax))
                {
                    // Send email logic here
                    await _emailService.SendEmailAsync(alert.Email, $"Alert for {parameterType}", $"The value for {parameterType} has reached {reading}");
                }
            }
        }
    }

}
