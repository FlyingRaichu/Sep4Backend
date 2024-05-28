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
    public async Task<ActionResult> AddTemplateAsync([FromBody] string name)
    {
        try
        {
            await _templateLogic.AddTemplate(name);
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

    [HttpPatch]
    public async Task<ActionResult> UpdateTemplateAsync(TemplateUpdateDto dto)
    {
        try
        {
            await _templateLogic.UpdateTemplate(dto);
            return Ok();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return StatusCode(500, e.Message);
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult> DeleteTemplateAsync([FromRoute] int id)
    {
        try
        {
            await _templateLogic.DeleteTemplate(id);
            return Ok();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return StatusCode(500, e.Message);
        }
    }

    [HttpPatch("/select/{id:int}")]
    public async Task<ActionResult> SelectTemplateAsync([FromBody] int id)
    {
        try
        {
            await _templateLogic.SelectTemplate(id);
            return Ok();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return StatusCode(500, e.Message);
        }
    }
}