using System.Text.Json.Serialization;
using DatabaseInterfacing.Converters;

namespace DatabaseInterfacing.Domain.DTOs;

[JsonConverter(typeof(MonitoringResultDtoConverter))]
public class MonitoringResultDto
{
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("readings")]
    public List<ReadingDto> Readings { get; set; }


    [JsonConstructor]
    public MonitoringResultDto()
    {
        
    }

    public MonitoringResultDto(string name, List<ReadingDto> readings)
    {
        Name = name;
        Readings = readings;
    }
}
