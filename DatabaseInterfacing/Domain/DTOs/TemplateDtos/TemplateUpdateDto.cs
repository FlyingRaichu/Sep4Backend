namespace DatabaseInterfacing.Domain.DTOs;

public class TemplateUpdateDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public IList<ParameterDto> Parameters { get; set; }
}