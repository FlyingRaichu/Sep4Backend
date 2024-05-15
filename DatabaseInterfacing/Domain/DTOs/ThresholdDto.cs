using System.Text.Json.Serialization;

namespace DatabaseInterfacing.Domain.DTOs;

public class ThresholdDto
{
    [JsonPropertyName("type")]
    public string Type { get; set; }
    [JsonPropertyName("min")]
    public double Min { get; set; }
    [JsonPropertyName("warning-min")]
    public double WarningMin { get; set; }
    [JsonPropertyName("warning-max")]
    public double WarningMax { get; set; }
    [JsonPropertyName("max")]
    public double Max { get; set; }
}