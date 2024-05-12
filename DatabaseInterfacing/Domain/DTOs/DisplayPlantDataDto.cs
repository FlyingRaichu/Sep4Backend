namespace DatabaseInterfacing.Domain.DTOs;

public class DisplayPlantDataDto
{
    public string Name { get; }
    public float WaterTemperature { get; }
    public float PhLevel { get; }
    public float WaterEC { get; }

    public DisplayPlantDataDto(string name, float waterTemperature, float phLevel, float waterEC)
    {
        Name = name;
        WaterTemperature = waterTemperature;
        PhLevel = phLevel;
        WaterEC = waterEC;
    }
}