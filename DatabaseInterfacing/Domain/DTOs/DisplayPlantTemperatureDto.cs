namespace DatabaseInterfacing.Domain.DTOs;

public class DisplayPlantTemperatureDto
{
    public int Id { get; }
    public float? WaterTemperature { get; }
    public string Status { get; }

    public DisplayPlantTemperatureDto(float? waterTemperature, string status)
    {
        WaterTemperature = waterTemperature;
        Status = status;
    }
}