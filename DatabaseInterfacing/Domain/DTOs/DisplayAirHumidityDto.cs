namespace DatabaseInterfacing.Domain.DTOs;

public class DisplayAirHumidityDto
{
    public float AirHumidityPercentage { get; set; }
    public string Status { get; set; }

    public DisplayAirHumidityDto(float airHumidityPercentage, string status)
    {
        AirHumidityPercentage = airHumidityPercentage;
        Status = status;
    }

    public DisplayAirHumidityDto()
    {
        AirHumidityPercentage = -90000;
        Status = "Error";
    }
}