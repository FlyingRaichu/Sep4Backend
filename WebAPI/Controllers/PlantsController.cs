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
        [FromQuery] float? waterEC,
        [FromQuery] float? waterFlow)
    {
        try
        {
            var searchDto = new SearchPlantDataDto(plantName, waterTemperature, phLevel, waterEC, waterFlow);
            var plants = await _logic.GetAsync(searchDto);
            return Ok(plants);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return StatusCode(500, e.Message);
        }
    }

    [HttpGet("/check")]
    public async Task<ActionResult<MonitoringResultDto>> GetAsync()
    {
        try
        {
            var response = await _logic.GetAllDataAsync();
            return Ok(response);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return StatusCode(500, e.Message);
        }
    }

    [HttpGet("/temperature")]
    public async Task<ActionResult<ActionResult<string>>> GetPlantTemperatureAsync()
    {
        try
        {
            var response = await _logic.CheckWaterTemperatureAsync();
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

    [HttpPatch("thresholds")]
    public async Task<IActionResult> UpdateThresholdConfigurationAsync([FromBody] ThresholdDto dto)
    {
        try
        {
            await _thresholdConfigurationService.UpdateConfigurationAsync(dto);
            return Ok();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return StatusCode(500, e.Message);
        }
    }
       
    [HttpGet("/ph")]
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
    
    [HttpGet("waterEC")]
    public async Task<ActionResult<string>> GetPlantEC()
    {
        try
        {
            var response = await _logic.CheckECAsync();
            return Ok(response);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return StatusCode(500, e.Message);
        }
    }
    
    [HttpGet("plant/flow")]
    public async Task<ActionResult<DisplayPlantWaterFlowDto>> CheckWaterFlowAsync()
    {
        try
        {
            DisplayPlantWaterFlowDto response = await _logic.CheckWaterFlowAsync();
            return Ok(response);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return StatusCode(500, e.Message);
        }
    }

    [HttpGet("waterLevel")]
    public async Task<ActionResult<DisplayPlantWaterLevelDto>> CheckWaterLevelAsync()
    {
        try
        {
            var response = await _logic.CheckWaterLevelAsync();
            return Ok(response);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return StatusCode(500, e.Message);
        }
    }

    [HttpGet("airTemperature")]
    public async Task<ActionResult<DisplayAirTemperatureDto>> CheckAirTemperatureAsync()
    {
        try
        {
            var response = await _logic.CheckAirTemperatureAsync();
            return Ok(response);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return StatusCode(500, e.Message);
        }
    }

    [HttpGet("airHumidity")]
    public async Task<ActionResult<DisplayAirHumidityDto>> CheckAirHumidityAsync()
    {
        try
        {
            var response = await _logic.CheckAirHumidityAsync();
            return Ok(response);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return StatusCode(500, e.Message);
        }
    }
    
    [HttpGet("airCO2")]
    public async Task<ActionResult<DisplayAirCO2Dto>> CheckAirCO2Async()
    {
        try
        {
            var response = await _logic.CheckAirCO2Async();
            return Ok(response);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return StatusCode(500, e.Message);
        }
    }

    [HttpGet("vpd")]
    public async Task<ActionResult<DisplayVPDLevelDto>> CheckVPDAsync()
    {
        try
        {
            var response = await _logic.CheckVPDAsync();
            return Ok(response);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return StatusCode(500, e.Message);
        }
    }

    [HttpGet("dewPoint")]
    public async Task<ActionResult<DisplayDewPointDto>> CheckDewPointAsync()
    {
        try
        {
            var response = await _logic.CheckDewPointAsync();
            return Ok(response);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return StatusCode(500, e.Message);
        }
    }
    
    [HttpGet("lightLevel")]
    public async Task<ActionResult<DisplayLightLevelDto>> CheckLightLevelsAsync()
    {
        try
        {
            var response = await _logic.CheckLightLevelAsync();
            return Ok(response);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return StatusCode(500, e.Message);
        }
    }
}