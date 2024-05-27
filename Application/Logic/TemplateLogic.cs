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
    public async Task AddTemplate(TemplateCreationDto creationDto)
    {
        var template = new Template
        {
            Name = creationDto.Name,
            Parameters = creationDto.Parameters.Select(t => new Parameter
            {
                Type = t.Type,
                Min = t.Min,
                WarningMin = t.WarningMin,
                WarningMax = t.WarningMax,
                Max = t.Max
            }).ToList()
        };
        _context.Templates.Add(template); 
        await _context.SaveChangesAsync();
        }

    public async Task<ICollection<TemplateDto>> GetAllAsync()
    {
        var templates = await _context.Templates
            .Include(t => t.Parameters) // Eager loading for Parameters
            .Select(t => new TemplateDto() // Map Template to TemplateDto
            {
                Id = t.Id,
                Name = t.Name,
                Parameters = t.Parameters.Select(p => new ParameterDto() // Map Parameter to ThresholdDto
                {
                    Id = p.Id,
                    Type = p.Type,
                    Min = p.Min,
                    WarningMin = p.WarningMin,
                    WarningMax = p.WarningMax,
                    Max = p.Max
                }).ToList()
            })
            .ToListAsync();
        return templates;
    }
    public async Task UpdateTemplateAsync(int id, IList<ParameterDto> updateDtos)
{
    var template = await _context.Templates
        .Include(t => t.Parameters)
        .FirstOrDefaultAsync(t => t.Id == id);
    if (template == null)
    {
        throw new Exception("Template not found");
    }

    foreach (var updateDto in updateDtos)
    {
        var parameter = template.Parameters.FirstOrDefault(p => p.Id == updateDto.Id);
        if (parameter != null)
        {
            parameter.Min = updateDto.Min;
            parameter.WarningMin = updateDto.WarningMin;
            parameter.WarningMax = updateDto.WarningMax;
            parameter.Max = updateDto.Max;
        }
    }

    await _context.SaveChangesAsync();
}
}