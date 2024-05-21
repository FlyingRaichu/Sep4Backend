namespace DatabaseInterfacing.Domain.DTOs;

public class DisplayLightLevelDto
{
    public float? LightLevel { get; set; }
    public string Status { get; set; }

    public DisplayLightLevelDto(float? lightLevel, string status)
    {
        LightLevel = lightLevel;
        Status = status;
    }
}