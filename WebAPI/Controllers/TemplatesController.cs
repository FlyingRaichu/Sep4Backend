using Application.LogicInterfaces;
using DatabaseInterfacing.Domain.DTOs;
using DatabaseInterfacing.Domain.EntityFramework;
using Microsoft.AspNetCore.Mvc;

namespace Sep4Backend.Controllers;

[ApiController]
[Route("[controller]")]
public class TemplatesController : ControllerBase
{
    private readonly ITemplateLogic _templateLogic;

    public TemplatesController(ITemplateLogic templateLogic)
    {
        _templateLogic = templateLogic;
    }

    [HttpPost]
    public async Task<ActionResult<Template>> AddTemplateAsync([FromBody] TemplateCreationDto creationDto)
    {
        try
        {
            await _templateLogic.AddTemplate(creationDto);
            return Ok();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return StatusCode(500, e.Message);
        }
    }

    [HttpGet]
    public async Task<ActionResult<ICollection<TemplateDto>>> GetAllAsync()
    {
        try
        {
            var templates = await _templateLogic.GetAllAsync();
            return Ok(templates);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return StatusCode(500, e.Message);
        }
    }
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateTemplateAsync(int id, [FromBody] IList<ParameterDto> updateDtos)
    {
        try
        {
            await _templateLogic.UpdateTemplateAsync(id, updateDtos);
            return Ok();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return StatusCode(500, e.Message);
        }
    }
}