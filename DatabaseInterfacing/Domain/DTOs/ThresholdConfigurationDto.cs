using System.Text.Json.Serialization;

namespace DatabaseInterfacing.Domain.DTOs;

public class ThresholdConfigurationDto
{
    [JsonPropertyName("thresholds")]
    public ICollection<ThresholdDto> Thresholds { get; set; }
    
    public ThresholdConfigurationDto()
    {
        Thresholds = new List<ThresholdDto>();
    }
}