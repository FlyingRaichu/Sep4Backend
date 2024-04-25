namespace DatabaseInterfacing.Domain.DTOs;

public class SearchPlantDataDto
{
    public string? PlantName { get; }
    public float? WaterTemperature { get; }
    public float? PHLevel { get; }

    public SearchPlantDataDto(string? plantName, float? waterTemperature, float? phLevel)
    {
        PlantName = plantName;
        WaterTemperature = waterTemperature;
        PHLevel = phLevel;
    }
}