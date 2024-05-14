using System.Text.Json.Serialization;
using DatabaseInterfacing.Domain.DTOs;

public class MonitoringResultDto
{
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("readings")]
    public List<ReadingDto> Readings { get; set; }

    public MonitoringResultDto(string name, List<ReadingDto> readings)
    {
        Name = name;
        Readings = readings;
    }
}