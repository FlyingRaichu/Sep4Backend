using System.Text.Json;
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
        private readonly IEmailService _emailService;
        private readonly IThresholdConfigurationService _thresholdConfigurationService;

        public AlertNotificationService(IEmailService emailService, IThresholdConfigurationService thresholdConfigurationService)
        {
            _emailService = emailService;
            _thresholdConfigurationService = thresholdConfigurationService;
        }

        public async Task CheckAndTriggerAlertsAsync(string parameterType, float? value)
        {
            if (value == null) return;

            var config = await _thresholdConfigurationService.GetConfigurationAsync();
            var threshold = config.Thresholds.FirstOrDefault(t => t.Type == parameterType);
            if (threshold == null) return;

            if (value >= threshold.Max || value <= threshold.Min || value >= threshold.WarningMax || value <= threshold.WarningMin)
            {
                var alertMessage = $"Alert: {parameterType} has reached a critical value of {value}. Thresholds: Min={threshold.Min}, Max={threshold.Max}, WarningMin={threshold.WarningMin}, WarningMax={threshold.WarningMax}.";
                await _emailService.SendEmailAsync("alert@example.com", $"Alert: {parameterType}", alertMessage); // Use actual recipient email address here
            }
        }

        public async Task CreateAlertNotificationAsync(AlertNotificationDto alertNotificationDto)
        {
            await using var dbContext = new PlantDbContext(DatabaseUtils.BuildConnectionOptions());
            var alertNotification = new AlertNotification
            {
                ParameterType = alertNotificationDto.ParameterType,
                ThresholdMin = alertNotificationDto.ThresholdMin,
                ThresholdMax = alertNotificationDto.ThresholdMax,
                WarningMin = alertNotificationDto.WarningMin,
                WarningMax = alertNotificationDto.WarningMax,
                Email = alertNotificationDto.Email
            };
            await dbContext.AlertNotifications.AddAsync(alertNotification);
            await dbContext.SaveChangesAsync();
        }

        public async Task<IEnumerable<AlertNotificationDto>> GetAllAlertNotificationsAsync()
        {
            await using var dbContext = new PlantDbContext(DatabaseUtils.BuildConnectionOptions());
            var alertNotifications = await dbContext.AlertNotifications.ToListAsync();
            return alertNotifications.Select(a => new AlertNotificationDto
            {
                Id = a.Id,
                ParameterType = a.ParameterType,
                ThresholdMin = a.ThresholdMin,
                ThresholdMax = a.ThresholdMax,
                WarningMin = a.WarningMin,
                WarningMax = a.WarningMax,
                Email = a.Email
            }).ToList();
        }

        public async Task UpdateAlertNotificationAsync(AlertNotificationDto alertNotificationDto)
        {
            await using var dbContext = new PlantDbContext(DatabaseUtils.BuildConnectionOptions());
            var alertNotification = await dbContext.AlertNotifications.FindAsync(alertNotificationDto.Id);
            if (alertNotification == null) throw new Exception("Alert notification not found.");

            alertNotification.ParameterType = alertNotificationDto.ParameterType;
            alertNotification.ThresholdMin = alertNotificationDto.ThresholdMin;
            alertNotification.ThresholdMax = alertNotificationDto.ThresholdMax;
            alertNotification.WarningMin = alertNotificationDto.WarningMin;
            alertNotification.WarningMax = alertNotificationDto.WarningMax;
            alertNotification.Email = alertNotificationDto.Email;

            dbContext.AlertNotifications.Update(alertNotification);
            await dbContext.SaveChangesAsync();
        }

        public async Task DeleteAlertNotificationAsync(int id)
        {
            await using var dbContext = new PlantDbContext(DatabaseUtils.BuildConnectionOptions());
            var alertNotification = await dbContext.AlertNotifications.FindAsync(id);
            if (alertNotification == null) throw new Exception("Alert notification not found.");

            dbContext.AlertNotifications.Remove(alertNotification);
            await dbContext.SaveChangesAsync();
        }
    }
}
