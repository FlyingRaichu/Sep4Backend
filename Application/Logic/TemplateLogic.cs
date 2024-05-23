using Application.LogicInterfaces;
using DatabaseInterfacing;
using DatabaseInterfacing.Context;
using DatabaseInterfacing.Domain.DTOs;
using DatabaseInterfacing.Domain.EntityFramework;

namespace Application.Logic;

public class TemplateLogic : ITemplateLogic
{
    public async Task<Template> AddTemplate(TemplateCreationDto dto)
    {
        await using var dbContext = new PlantDbContext(DatabaseUtils.BuildConnectionOptions());
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
        //dbContext.Templates.Add(template); 
        //await dbContext.SaveChangesAsync(); 
        return template;
    }
}