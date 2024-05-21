using System.Text.Json.Serialization;

namespace DatabaseInterfacing.Domain.DTOs
{
    public class ReadingDto
    {
        [JsonPropertyName("water_conductivity")]
        public float? WaterConductivity { get; set; }

        [JsonPropertyName("water_temperature")]
        public float? WaterTemperature { get; set; }

        [JsonPropertyName("water_ph")]
        public float? WaterPhLevel { get; set; }

        [JsonPropertyName("water_flow")] 
        public float? WaterFlow { get; set; }

        [JsonPropertyName("water_level")] 
        public float? WaterLevel { get; set; }

        [JsonPropertyName("air_temperature")] 
        public float? AirTemperature { get; set; }

        [JsonPropertyName("air_humidity")] 
        public float? AirHumidity { get; set; }
        
        [JsonPropertyName("air_co2")]
        
        public float? AirCO2 { get; set; }

        [JsonPropertyName("light_level")] 
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