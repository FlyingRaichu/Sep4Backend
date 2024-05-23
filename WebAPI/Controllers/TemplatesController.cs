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
    public async Task<ActionResult<Template>> AddTemplateAsync([FromBody] TemplateCreationDto dto)
    {
        try
        {
            var template = await _templateLogic.AddTemplate(dto);
            return Ok(template);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return StatusCode(500, e.Message);
        }
    }

    [HttpGet]
    public async Task<ActionResult<ICollection<Template>>> GetAllAsync()
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
}