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

        public AlertNotificationLogic(PlantDbContext context)
        {
            _context = context;
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
                Email = alert.Email,
                IsThresholdMinEnabled = alert.ThresholdMin > double.NegativeInfinity,
                IsThresholdMaxEnabled = alert.ThresholdMax < double.PositiveInfinity
            };
        }

        public async Task UpdateAlertNotificationAsync(int id, bool isThresholdMinEnabled, bool isThresholdMaxEnabled)
        {
            var alert = await _context.AlertNotifications.FindAsync(id);
            if (alert == null) throw new Exception("Alert notification not found");

            alert.ThresholdMin = isThresholdMinEnabled ? alert.ThresholdMin : double.NegativeInfinity;
            alert.ThresholdMax = isThresholdMaxEnabled ? alert.ThresholdMax : double.PositiveInfinity;

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
                    Email = a.Email,
                    IsThresholdMinEnabled = a.ThresholdMin > double.NegativeInfinity,
                    IsThresholdMaxEnabled = a.ThresholdMax < double.PositiveInfinity
                }).ToListAsync();
        }
    }
}
