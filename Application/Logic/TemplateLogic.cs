using Application.LogicInterfaces;
using Application.ServiceInterfaces;
using DatabaseInterfacing;
using DatabaseInterfacing.Context;
using DatabaseInterfacing.Domain;
using DatabaseInterfacing.Domain.DTOs;
using DatabaseInterfacing.Domain.EntityFramework;
using Microsoft.EntityFrameworkCore;

namespace Application.Logic;

public class TemplateLogic : ITemplateLogic
{
    private readonly PlantDbContext _context;
    private IThresholdConfigurationService _thresholdConfiguration;

    public TemplateLogic(IThresholdConfigurationService thresholdConfiguration)
    {
        _thresholdConfiguration = thresholdConfiguration;
        _context = new PlantDbContext(DatabaseUtils.BuildConnectionOptions());
    }
    public async Task AddTemplate(string name)
    {
        try
        {
            var waterParameterTypes = new List<string>()
            {
                "waterPh", "waterLevel", "waterConductivity", "waterTemperature",
                "waterFlow", "airTemperature", "airHumidity", "airCo2", "vpdLevel", "dewPoint", "lightLevel"
            };
            var template = new Template
            {
                Name = name
            };
            foreach (var type in waterParameterTypes)
            {
                template.Parameters.Add(new Parameter()
                {
                    Min = 0,
                    Max = 0,
                    WarningMax = 0,
                    WarningMin = 0,
                    Type = type
                });
            }
            
            _context.Templates.Add(template); 
            await _context.SaveChangesAsync();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public async Task<ICollection<TemplateDto>> GetAllAsync()
    {
        try
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
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public async Task UpdateTemplate(TemplateUpdateDto dto)
    {
        try
        {
            var template = await _context.Templates
                .Include(t => t.Parameters) 
                .FirstOrDefaultAsync(t => t.Id == dto.Id);
            if (template == null) throw new Exception($"Template with Id {dto.Id} not found");
            template.Name = dto.Name;
            UpdateTemplateParameters(template, dto.Parameters);
            await _context.SaveChangesAsync();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public async Task DeleteTemplate(int id)
    {
        try
        {
            var template = await _context.Templates.FirstOrDefaultAsync(t => t.Id == id);
            if (template == null) throw new Exception($"Template with Id {id} not found");
            _context.Templates.Remove(template);
            await _context.SaveChangesAsync();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public async Task SelectTemplate(int id)
    {
        try
        {
            var template = await _context.Templates
                .Include(t => t.Parameters) // Eager loading for Parameters (optional)
                .FirstOrDefaultAsync(t => t.Id == id);
            if (template == null) throw new Exception($"Template with Id {id} not found");
            foreach (var parameter in template.Parameters)
            {
                await _thresholdConfiguration.UpdateConfigurationAsync(new ThresholdDto()
                {
                    Max = parameter.Max,
                    Min = parameter.Min,
                    Type = parameter.Type,
                    WarningMax = parameter.WarningMax,
                    WarningMin = parameter.WarningMin
                });
            }
            await _thresholdConfiguration.ChooseTemplate(template.Name);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    private void UpdateTemplateParameters(Template template, ICollection<ParameterDto> parameterDtos)
    {
        var waterParameterTypes = new List<string>()
        {
            "waterPh", "waterLevel", "waterConductivity", "waterTemperature",
            "waterFlow", "airTemperature", "airHumidity", "airCo2"
        };

        foreach (var parameterDto in parameterDtos)
        {
            var existingParameter = template.Parameters.FirstOrDefault(p => p.Id == parameterDto.Id);

            if (existingParameter != null)
            {
                existingParameter.Type = parameterDto.Type;
                existingParameter.Min = parameterDto.Min;
                existingParameter.WarningMin = parameterDto.WarningMin;
                existingParameter.WarningMax = parameterDto.WarningMax;
                existingParameter.Max = parameterDto.Max;
            }
            else if (parameterDto != null && waterParameterTypes.Contains(parameterDto.Type))
            {
                if (!template.Parameters.Any(p => p.Type == parameterDto.Type && p.Id != parameterDto.Id))
                {
                    template.Parameters.Add(new Parameter
                    {
                        Type = parameterDto.Type,
                        Min = parameterDto.Min,
                        WarningMin = parameterDto.WarningMin,
                        WarningMax = parameterDto.WarningMax,
                        Max = parameterDto.Max
                    });
                }
            }
        }
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