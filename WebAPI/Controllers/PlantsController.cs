using Application.LogicInterfaces;
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

    public PlantsController(IPlantDataLogic logic, IConfiguration configuration)
    {
        _logic = logic;
        _configuration = configuration;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<PlantData>>> GetAsync([FromQuery] string? plantName,
        [FromQuery] float? waterTemperature,
        [FromQuery] float? phLevel,
        [FromQuery] float? waterFlow)
    {
        try
        {
            var searchDto = new SearchPlantDataDto(plantName, waterTemperature, phLevel, waterFlow);
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
    public async Task<ActionResult<ActionResult<string>>> GetPlantTemperature(int id)
    {
        try
        {
            var response = await _logic.CheckTemperatureAsync(id);
            Console.WriteLine($"Water temp is: {response.WaterTemperature}");
            return Ok(response);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return StatusCode(500, e.Message);
        }
    }

    [HttpGet("plant/ph")]
    public async Task<ActionResult<DisplayPlantPhDto>> CheckPhLevelAsync()
    {
        try
        {
            DisplayPlantPhDto response = await _logic.CheckPhLevelAsync();
            return Ok(response);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return StatusCode(500, e.Message);
        }
    }
}