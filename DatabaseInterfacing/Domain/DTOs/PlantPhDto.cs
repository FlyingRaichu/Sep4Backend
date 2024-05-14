namespace DatabaseInterfacing.Domain.DTOs;

public class PlantPhDto
{
    public int PlantId { get; set; }
    public double PhLevel { get; set; }
    public bool IsOkay { get; set; }

    public PlantPhDto()
    {
        IsOkay = false;
    }
}