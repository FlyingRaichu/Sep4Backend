namespace DatabaseInterfacing.Domain.DTOs;

public class TemplateCreationDto
{
    public string Name { get; set; }
    public IList<ThresholdDto> Parameters { get; set; }

    public TemplateCreationDto(string name, IList<ThresholdDto> parameters)
    {
        Name = name;
        Parameters = parameters;
    }
}