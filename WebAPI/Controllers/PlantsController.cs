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
        [FromQuery] float? phLevel)
    {
        try
        {
            var searchDto = new SearchPlantDataDto(plantName, waterTemperature, phLevel);
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
            Console.WriteLine($"Water temp is: {response.WaterTemperature}" );
    
    [HttpGet("plants/{id}/ph")]
    public async Task<ActionResult<PlantPhDto>> CheckPhLevelAsync(int id)
    {
        try
        {
            PlantPhDto response = await _logic.CheckPhLevelAsync(id);
            return Ok(response);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return StatusCode(500, e.Message);
        }
    }
}