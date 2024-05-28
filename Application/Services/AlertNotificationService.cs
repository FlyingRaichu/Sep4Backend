using System.Threading.Tasks;
using Application.ServiceInterfaces;
using DatabaseInterfacing.Domain.EntityFramework;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using DatabaseInterfacing.Context;
using Microsoft.EntityFrameworkCore;

namespace Application.Services;

    public class AlertNotificationService : IAlertNotificationService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IEmailService _emailService;

        public AlertNotificationService(IServiceScopeFactory scopeFactory, IEmailService emailService)
        {
            _scopeFactory = scopeFactory;
            _emailService = emailService;
        }

        public async Task CheckAndTriggerAlertsAsync(string parameterType, float? value)
        {
            if (value == null) return;

            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<PlantDbContext>();
            var alerts = await context.AlertNotifications
                .Where(a => a.ParameterType == parameterType)
                .ToListAsync();

            var alertGroups = alerts
                .Where(alert => value > alert.ThresholdMax || value < alert.ThresholdMin || 
                                value > alert.WarningMax || value < alert.WarningMin)
                .GroupBy(alert => alert.Email);

            foreach (var group in alertGroups)
            {
                var messages = new List<string>();
                foreach (var alert in group)
                {
                    string type = value > alert.ThresholdMax || value < alert.ThresholdMin ? "Threshold" : "Warning";
                    string message = $"{type} Alert for {alert.ParameterType}: The value has reached {value}, which is outside the defined {type} limits.\n\n" +
                                     $"ThresholdMin: {alert.ThresholdMin}\n" +
                                     $"ThresholdMax: {alert.ThresholdMax}\n" +
                                     $"WarningMin: {alert.WarningMin}\n" +
                                     $"WarningMax: {alert.WarningMax}\n";
                    messages.Add(message);
                }
                await SendCombinedAlertEmail(group.Key, messages);
            }
        }

        private async Task SendCombinedAlertEmail(string email, List<string> messages)
        {
            var subject = "Multiple Alerts Notification";
            var body = string.Join("\n\n", messages);
            await _emailService.SendEmailAsync(email, subject, body);
        }
    }