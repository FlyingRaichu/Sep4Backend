namespace DatabaseInterfacing.Domain.DTOs;

public class SearchPlantDataDto
{
    public string? PlantName { get; }
    public float? WaterTemperature { get; }
    public float? PHLevel { get; }
    public float? WaterEC { get; }

    public SearchPlantDataDto(string? plantName, float? waterTemperature, float? phLevel, float? waterEC)
    {
        PlantName = plantName;
        WaterTemperature = waterTemperature;
        PHLevel = phLevel;
        WaterEC = waterEC;
    }
}