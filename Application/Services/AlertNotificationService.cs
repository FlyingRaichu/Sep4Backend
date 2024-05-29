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
        private readonly PlantDbContext _context;
        private readonly IEmailService _emailService;

        public AlertNotificationService(PlantDbContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        public async Task<IEnumerable<AlertNotificationDto>> GetAllAsync()
        {
            return await _context.AlertNotifications
                .Select(alert => new AlertNotificationDto
                {
                    Id = alert.Id,
                    ParameterType = alert.ParameterType,
                    ThresholdMin = alert.ThresholdMin,
                    ThresholdMax = alert.ThresholdMax,
                    Email = alert.Email
                })
                .ToListAsync();
        }

        public async Task UpdateAlertNotificationAsync(int id, AlertNotificationDto updateDto)
        {
            var alert = await _context.AlertNotifications.FirstOrDefaultAsync(a => a.Id == id);

            if (alert == null)
            {
                throw new KeyNotFoundException("Alert notification not found");
            }

            alert.ThresholdMin = updateDto.ThresholdMin;
            alert.ThresholdMax = updateDto.ThresholdMax;
            alert.Email = updateDto.Email;

            _context.AlertNotifications.Update(alert);
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
