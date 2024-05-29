using Application.LogicInterfaces;
using Application.ServiceInterfaces;
using Application.Services;
using DatabaseInterfacing.Context;
using DatabaseInterfacing.Domain.DTOs;
using DatabaseInterfacing.Domain.EntityFramework;
using Microsoft.EntityFrameworkCore;

namespace Application.Logic
{
    public class AlertNotificationLogic : IAlertNotificationLogic
    {
        private readonly PlantDbContext _context;
        private readonly IEmailService _emailService;

        public AlertNotificationLogic(PlantDbContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        public async Task<AlertNotificationDto> GetAlertNotificationAsync(int id)
        {
            var alert = await _context.AlertNotifications
                .Where(a => a.Id == id)
                .FirstOrDefaultAsync();

            if (alert == null) throw new KeyNotFoundException("Alert notification not found");

            return new AlertNotificationDto
            {
                Id = alert.Id,
                ParameterType = alert.ParameterType,
                ThresholdMin = alert.ThresholdMin,
                ThresholdMax = alert.ThresholdMax,
                Email = alert.Email
            };
        }

        public async Task UpdateAlertNotificationAsync(int id, double thresholdMin, double thresholdMax)
        {
            var alert = await _context.AlertNotifications.FindAsync(id);
            if (alert == null) throw new Exception("Alert notification not found");

            alert.ThresholdMin = thresholdMin;
            alert.ThresholdMax = thresholdMax;

            await _context.SaveChangesAsync();
        }

        public async Task CheckAndTriggerAlertsAsync(string parameterType, double? reading)
        {
            if (reading == null)
                return;

            var alerts = await _context.AlertNotifications
                .Where(a => a.ParameterType == parameterType)
                .ToListAsync();

            foreach (var alert in alerts)
            {
                if (reading <= alert.ThresholdMin || reading >= alert.ThresholdMax)
                {
                    // Send email logic here
                    await _emailService.SendEmailAsync(alert.Email, $"Alert for {parameterType}", $"The value for {parameterType} has reached {reading}");
                }
            }
        }
    }
}
