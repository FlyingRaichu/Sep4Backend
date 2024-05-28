namespace DatabaseInterfacing.Domain.DTOs;

public class SearchPlantDataDto
{
    public string? PlantName { get; }
    public float? WaterConductivity { get; }
    public float? WaterTemperature { get; }
    public float? WaterPhLevel { get; }
    public float? WaterFlow { get; }
    public float? WaterLevel { get; }
    public float? AirTemperature { get; }
    public float? AirHumidity { get; }
    public float? AirCO2 { get; }
    public float? LightLevel { get; }
    public float? DewPoint { get; }
    public float? VpdLevel { get; }
    public DateTime? DateTime { get; }

    public SearchPlantDataDto(string? plantName, float? waterConductivity, float? waterTemperature, float? waterPhLevel, float? waterFlow, float? waterLevel, float? airTemperature, float? airHumidity, float? airCO2, float? lightLevel, float? dewPoint, float? vpdLevel, DateTime? dateTime)
    {
        PlantName = plantName;
        WaterConductivity = waterConductivity;
        WaterTemperature = waterTemperature;
        WaterPhLevel = waterPhLevel;
        WaterFlow = waterFlow;
        WaterLevel = waterLevel;
        AirTemperature = airTemperature;
        AirHumidity = airHumidity;
        AirCO2 = airCO2;
        LightLevel = lightLevel;
        DewPoint = dewPoint;
        VpdLevel = vpdLevel;
        DateTime = dateTime;
    }
}