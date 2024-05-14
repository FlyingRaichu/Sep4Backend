namespace DatabaseInterfacing.Domain.DTOs;

public class PlantWaterFlowDto
{
    public int PlantId { get; set; }
    public float WaterFlow { get; set; }
    public bool IsOkay { get; set; }

    public PlantWaterFlowDto()
    {
        IsOkay = false;
    }
}