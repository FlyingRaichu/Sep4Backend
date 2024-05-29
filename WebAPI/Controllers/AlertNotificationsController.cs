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
        private readonly IAlertNotificationLogic _alertNotificationLogic;

        public AlertNotificationsController(IAlertNotificationLogic alertNotificationLogic)
        {
            _alertNotificationLogic = alertNotificationLogic;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<AlertNotificationDto>> GetAsync(int id)
        {
            try
            {
                var alert = await _alertNotificationLogic.GetAlertNotificationAsync(id);
                return Ok(alert);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPatch("{id}")]
        public async Task<IActionResult> UpdateAlertNotificationAsync(int id, [FromBody] AlertNotificationDto updateDto)
        {
            try
            {
                await _alertNotificationLogic.UpdateAlertNotificationAsync(id, updateDto.IsThresholdMinEnabled, updateDto.IsThresholdMaxEnabled);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }

}