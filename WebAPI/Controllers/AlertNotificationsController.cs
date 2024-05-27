using Application.LogicInterfaces;
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

        [HttpPost]
        public async Task<IActionResult> CreateAlertNotificationAsync([FromBody] AlertNotificationDto dto)
        {
            try
            {
                await _alertNotificationLogic.CreateAlertNotificationAsync(dto);
                return Ok();
            }
            catch (Exception e)
            {
                return StatusCode(500, e.Message);
            }
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<AlertNotificationDto>>> GetAlertNotificationsAsync()
        {
            try
            {
                var alerts = await _alertNotificationLogic.GetAlertNotificationsAsync();
                return Ok(alerts);
            }
            catch (Exception e)
            {
                return StatusCode(500, e.Message);
            }
        }

        [HttpPut]
        public async Task<IActionResult> UpdateAlertNotificationAsync([FromBody] AlertNotificationDto dto)
        {
            try
            {
                await _alertNotificationLogic.UpdateAlertNotificationAsync(dto);
                return Ok();
            }
            catch (Exception e)
            {
                return StatusCode(500, e.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAlertNotificationAsync(int id)
        {
            try
            {
                await _alertNotificationLogic.DeleteAlertNotificationAsync(id);
                return Ok();
            }
            catch (Exception e)
            {
                return StatusCode(500, e.Message);
            }
        }
    }
}