namespace DatabaseInterfacing.Domain.DTOs;

public class DisplayPlantWaterLevelDto
{
    public float WaterLevelInMillimeters { get; set; }
    public string Status { get; set; }

    public DisplayPlantWaterLevelDto(float waterLevelInMillimeters, string status)
    {
        WaterLevelInMillimeters = waterLevelInMillimeters;
        Status = status;
    }

    public DisplayPlantWaterLevelDto()
    {
        WaterLevelInMillimeters = -90000;
        Status = "Error";
    }
}