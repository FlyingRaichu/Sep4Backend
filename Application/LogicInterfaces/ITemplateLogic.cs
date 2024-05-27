using DatabaseInterfacing.Domain.DTOs;
using DatabaseInterfacing.Domain.EntityFramework;

namespace Application.LogicInterfaces;

public interface ITemplateLogic
{
    public Task AddTemplate(TemplateCreationDto creationDto);
    public Task<ICollection<TemplateDto>> GetAllAsync();

    public Task UpdateTemplateAsync(int id, IList<ParameterDto> updateDtos);
}