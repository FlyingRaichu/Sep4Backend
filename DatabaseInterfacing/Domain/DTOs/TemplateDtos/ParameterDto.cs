using System.Text.Json.Serialization;

namespace DatabaseInterfacing.Domain.DTOs;

public class ParameterDto
{
    public int Id { get; set; }
    [JsonPropertyName("type")]
    public string Type { get; set; }
    [JsonPropertyName("min")]
    public double Min { get; set; }
    [JsonPropertyName("warningMin")]
    public double WarningMin { get; set; }
    [JsonPropertyName("warningMax")]
    public double WarningMax { get; set; }
    [JsonPropertyName("max")]
    public double Max { get; set; }
}