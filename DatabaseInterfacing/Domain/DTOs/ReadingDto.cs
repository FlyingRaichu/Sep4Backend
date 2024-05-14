using System.Text.Json.Serialization;

namespace DatabaseInterfacing.Domain.DTOs;

public class ReadingDto
{
    [JsonPropertyName("water_conductivity")]
    public float? WaterConductivity { get; set; }
    
    [JsonPropertyName("water_temperature")]
    public float? WaterTemperature { get; set; }

    
    [JsonConstructor]
    public ReadingDto(float? waterConductivity, float? waterTemperature)
    {
        WaterConductivity = waterConductivity;
        WaterTemperature = waterTemperature;
    }
}