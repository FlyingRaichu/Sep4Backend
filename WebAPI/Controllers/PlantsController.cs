﻿using Application.LogicInterfaces;
using DatabaseInterfacing.Domain.EntityFramework;
using Microsoft.AspNetCore.Mvc;

namespace Sep4Backend.Controllers;

[ApiController]
[Route("[controller]")]
public class PlantsController : ControllerBase
{
    private readonly IPlantDataLogic _logic;

    public PlantsController(IPlantDataLogic logic)
    {
        _logic = logic;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<PlantData>>> GetAsync()
    {
        try
        {
            var plants = await _logic.GetAsync();
            return Ok(plants);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return StatusCode(500, e.Message);
        }
    }
}