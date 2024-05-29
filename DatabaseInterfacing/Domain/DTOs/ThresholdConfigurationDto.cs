using System.Text.Json.Serialization;

namespace DatabaseInterfacing.Domain.DTOs;

public class ThresholdConfigurationDto
{
    [JsonPropertyName("templateName")]
    public string TemplateName { get; set; }
    [JsonPropertyName("thresholds")]
    public ICollection<ThresholdDto> Thresholds { get; set; }
    
    public ThresholdConfigurationDto()
    {
        Thresholds = new List<ThresholdDto>();
    }
}