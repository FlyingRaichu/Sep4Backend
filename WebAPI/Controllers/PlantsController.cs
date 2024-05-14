using Application.LogicInterfaces;
using Application.ServiceInterfaces;
using DatabaseInterfacing.Domain.DTOs;
using DatabaseInterfacing.Domain.EntityFramework;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Sep4Backend.Controllers;

[ApiController]
[Route("[controller]")]
public class PlantsController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly IPlantDataLogic _logic;
    private readonly IThresholdConfigurationService _thresholdConfigurationService;

    public PlantsController(IPlantDataLogic logic, IConfiguration configuration, IThresholdConfigurationService thresholdConfigurationService)
    {
        _logic = logic;
        _configuration = configuration;
        _thresholdConfigurationService = thresholdConfigurationService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<PlantData>>> GetAsync([FromQuery] string? plantName,
        [FromQuery] float? waterTemperature,
        [FromQuery] float? phLevel,
        [FromQuery] float? waterEC)
    {
        try
        {
            var searchDto = new SearchPlantDataDto(plantName, waterTemperature, phLevel, waterEC);
            var plants = await _logic.GetAsync(searchDto);
            return Ok(plants);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return StatusCode(500, e.Message);
        }
    }

    [HttpGet("{id:int}/temperature")]
    public async Task<ActionResult<ActionResult<string>>> GetPlantTemperatureAsync(int id)
    {
        try
        {
            var response = await _logic.CheckTemperatureAsync(id);
            return Ok(response);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return StatusCode(500, e.Message);
        }
    }

    [HttpGet("thresholds")]
    public async Task<ActionResult<ThresholdConfigurationDto>> GetThresholdsAsync()
    {
        try
        {
            var response = await _thresholdConfigurationService.GetConfigurationAsync();
            return Ok(response);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return StatusCode(500, e.Message);
        }
    }

    [HttpPut]
    public async Task<IActionResult> UpdateThresholdConfigurationAsync([FromBody] ThresholdConfigurationDto configurationDto)
    {
        try
        {
            await _thresholdConfigurationService.UpdateConfigurationAsync(configurationDto);
            return Ok();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return StatusCode(500, e.Message);
        }
    }
       
    [HttpGet("plants/{id}/ph")]
    public async Task<ActionResult<DisplayPlantPhDto>> CheckPhLevelAsync()
    {
        try
        {
            DisplayPlantPhDto response = await _logic.GetPhLevelAsync();
            return Ok(response);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return StatusCode(500, e.Message);
        }
    }
    
    [HttpGet("{id:int}/waterEC")]
    public async Task<ActionResult<string>> GetPlantEC(int id)
    {
        try
        {
            var response = await _logic.CheckECAsync(id);
            return Ok(response);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return StatusCode(500, e.Message);
        }
    }
}