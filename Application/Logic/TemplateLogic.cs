using Application.LogicInterfaces;
using DatabaseInterfacing;
using DatabaseInterfacing.Context;
using DatabaseInterfacing.Domain.DTOs;
using DatabaseInterfacing.Domain.EntityFramework;
using Microsoft.EntityFrameworkCore;

namespace Application.Logic;

public class TemplateLogic : ITemplateLogic
{
    private PlantDbContext _context;

    public TemplateLogic()
    {
        _context = new PlantDbContext(DatabaseUtils.BuildConnectionOptions());
    }
    public async Task<Template> AddTemplate(TemplateCreationDto dto)
    {
        var template = new Template
        {
            Name = dto.Name,
            Parameters = dto.Parameters.Select(t => new Parameter
            {
                Type = t.Type,
                Min = t.Min,
                WarningMin = t.WarningMin,
                WarningMax = t.WarningMax,
                Max = t.Max
            }).ToList()
        };
        Console.WriteLine(template);
        //_context.Templates.Add(template); 
        //await _context.SaveChangesAsync(); 
        return template;
    }

    public async Task<ICollection<Template>> GetAllAsync()
    {
        var templates = await _context.Templates
            .Include(t => t.Parameters) 
            .ToListAsync(); 
        return templates;
    }
}