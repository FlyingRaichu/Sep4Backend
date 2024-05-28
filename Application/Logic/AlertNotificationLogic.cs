using Application.LogicInterfaces;
using DatabaseInterfacing.Domain.DTOs;
using DatabaseInterfacing.Domain.EntityFramework;
using Microsoft.EntityFrameworkCore;
using Application.ServiceInterfaces;
using DatabaseInterfacing.Context;

namespace Application.Services
{
    public class AlertNotificationLogic : IAlertNotificationLogic
    {
        private readonly PlantDbContext _context;

        public AlertNotificationLogic(PlantDbContext context)
        {
            _context = context;
        }

        public async Task CreateAlertNotificationAsync(AlertNotificationDto dto)
        {
            var alert = new AlertNotification
            {
                ParameterType = dto.ParameterType,
                ThresholdMin = dto.ThresholdMin,
                ThresholdMax = dto.ThresholdMax,
                WarningMin = dto.WarningMin,
                WarningMax = dto.WarningMax,
                Email = dto.Email
            };
            _context.AlertNotifications.Add(alert);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<AlertNotificationDto>> GetAlertNotificationsAsync()
        {
            return await _context.AlertNotifications
                .Select(a => new AlertNotificationDto
                {
                    Id = a.Id,
                    ParameterType = a.ParameterType,
                    ThresholdMin = a.ThresholdMin,
                    ThresholdMax = a.ThresholdMax,
                    WarningMin = a.WarningMin,
                    WarningMax = a.WarningMax,
                    Email = a.Email
                }).ToListAsync();
        }

        public async Task UpdateAlertNotificationAsync(AlertNotificationDto dto)
        {
            var alert = await _context.AlertNotifications.FindAsync(dto.Id);
            if (alert == null) throw new Exception("Alert notification not found");

            alert.ParameterType = dto.ParameterType;
            alert.ThresholdMin = dto.ThresholdMin;
            alert.ThresholdMax = dto.ThresholdMax;
            alert.WarningMin = dto.WarningMin;
            alert.WarningMax = dto.WarningMax;
            alert.Email = dto.Email;

            await _context.SaveChangesAsync();
        }

        public async Task DeleteAlertNotificationAsync(int id)
        {
            var alert = await _context.AlertNotifications.FindAsync(id);
            if (alert == null) throw new Exception("Alert notification not found");

            _context.AlertNotifications.Remove(alert);
            await _context.SaveChangesAsync();
        }
    }
}