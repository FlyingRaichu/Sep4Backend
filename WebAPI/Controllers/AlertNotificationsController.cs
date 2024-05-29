using Application.LogicInterfaces;
using Application.ServiceInterfaces;
using DatabaseInterfacing.Domain.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace Sep4Backend.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AlertNotificationsController : ControllerBase
    {
        private readonly IAlertNotificationService _alertNotificationService;

        public AlertNotificationsController(IAlertNotificationService alertNotificationService)
        {
            _alertNotificationService = alertNotificationService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<AlertNotificationDto>>> GetAsync()
        {
            var alerts = await _alertNotificationService.GetAllAsync();
            return Ok(alerts);
        }

        [HttpPatch("{id}")]
        public async Task<IActionResult> UpdateAlertNotificationAsync(int id, [FromBody] AlertNotificationDto updateDto)
        {
            try
            {
                await _alertNotificationService.UpdateAlertNotificationAsync(id, updateDto);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}