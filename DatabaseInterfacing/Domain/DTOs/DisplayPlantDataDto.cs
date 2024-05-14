namespace DatabaseInterfacing.Domain.DTOs;

public class DisplayPlantDataDto
{
    public string Name { get; }
    public float WaterTemperature { get; }
    public float PhLevel { get; }
    public float WaterFlow { get; }

    public DisplayPlantDataDto(string name, float waterTemperature, float phLevel, float waterFlow)
    {
        Name = name;
        WaterTemperature = waterTemperature;
        PhLevel = phLevel;
        WaterFlow = waterFlow;
    }
}