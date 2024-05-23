using DatabaseInterfacing.Domain.DTOs;
using DatabaseInterfacing.Domain.EntityFramework;

namespace Application.LogicInterfaces;

public interface ITemplateLogic
{
    public Task<Template> AddTemplate(TemplateCreationDto dto);
}