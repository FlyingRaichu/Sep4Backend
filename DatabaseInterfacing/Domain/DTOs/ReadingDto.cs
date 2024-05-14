using System.Text.Json.Serialization;

namespace DatabaseInterfacing.Domain.DTOs;

public class ReadingDto
{
    [JsonPropertyName("water_conductivity")]
    public int? WaterConductivity { get; set; }
    
    [JsonPropertyName("water_temperature")]
    public float? WaterTemperature { get; set; }
    
    [JsonPropertyName("water_ph")]
    public float? WaterPhLevel { get; set; }

    
    [JsonConstructor]
    public ReadingDto(int? waterConductivity, float? waterTemperature, float? waterPhLevel)
    {
        WaterConductivity = waterConductivity;
        WaterTemperature = waterTemperature;
        WaterPhLevel = waterPhLevel;
    }
}