namespace DatabaseInterfacing.Domain.DTOs;

public class DisplayAirTemperatureDto
{
    public float AirTemperatureInC { get; set; }
    public string Status { get; set; }

    public DisplayAirTemperatureDto(float airTemperatureInC, string status)
    {
        AirTemperatureInC = airTemperatureInC;
        Status = status;
    }

    public DisplayAirTemperatureDto()
    {
        AirTemperatureInC = -90000;
        Status = "Error";
    }
}