using System.Text.Json.Serialization;

namespace DatabaseInterfacing.Domain.DTOs;

public class ThresholdDto
{
    [JsonPropertyName("type")]
    public string Type { get; set; }
    [JsonPropertyName("perfect-min")]
    public double PerfectMin { get; set; }
    [JsonPropertyName("warning-min")]
    public double WarningMin { get; set; }
    [JsonPropertyName("warning-max")]
    public double WarningMax { get; set; }
    [JsonPropertyName("perfect-max")]
    public double PerfectMax { get; set; }
}