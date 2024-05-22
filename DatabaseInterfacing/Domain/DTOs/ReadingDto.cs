using System.Text.Json.Serialization;

namespace DatabaseInterfacing.Domain.DTOs
{
    public class ReadingDto
    {
        [JsonPropertyName("waterConductivity")]
        public float? WaterConductivity { get; set; }

        [JsonPropertyName("waterTemperature")]
        public float? WaterTemperature { get; set; }

        [JsonPropertyName("waterPh")]
        public float? WaterPhLevel { get; set; }

        [JsonPropertyName("waterFlow")] 
        public float? WaterFlow { get; set; }

        [JsonPropertyName("waterLevel")] 
        public float? WaterLevel { get; set; }

        [JsonPropertyName("airTemperature")] 
        public float? AirTemperature { get; set; }

        [JsonPropertyName("airHumidity")] 
        public float? AirHumidity { get; set; }
        
        [JsonPropertyName("airCo2")]
        
        public float? AirCO2 { get; set; }

        [JsonPropertyName("lightLevel")] 
        public float? LightLevel { get; set; }

        [JsonConstructor]
        public ReadingDto(float? waterConductivity, float? waterTemperature, float? waterPhLevel, float? waterFlow,
            float? waterLevel, float? airTemperature, float? airHumidity, float? airCO2, float? lightLevel)
        {
            WaterConductivity = waterConductivity;
            WaterTemperature = waterTemperature;
            WaterPhLevel = waterPhLevel;
            WaterFlow = waterFlow;
            WaterLevel = waterLevel;
            AirTemperature = airTemperature;
            AirHumidity = airHumidity;
            AirCO2 = airCO2;
            LightLevel = lightLevel;
        }
    }
}