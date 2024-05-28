using System.Text.Json;
using System.Text.Json.Serialization;

namespace DatabaseInterfacing.Domain.DTOs;

public class ReadingDto
{
    [JsonExtensionData]
    public Dictionary<string, JsonElement> Measurements { get; set; }

    
    [JsonConstructor]
    public ReadingDto()
    {
        Measurements = new Dictionary<string, JsonElement>();
    }
}