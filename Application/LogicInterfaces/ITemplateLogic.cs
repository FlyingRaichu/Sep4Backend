using DatabaseInterfacing.Domain.DTOs;
using DatabaseInterfacing.Domain.EntityFramework;

namespace Application.LogicInterfaces;

public interface ITemplateLogic
{
    public Task AddTemplate(string name);
    public Task<ICollection<TemplateDto>> GetAllAsync();
    public Task UpdateTemplate(TemplateUpdateDto dto);
    public Task DeleteTemplate(int id);
    public Task SelectTemplate(int id);
}