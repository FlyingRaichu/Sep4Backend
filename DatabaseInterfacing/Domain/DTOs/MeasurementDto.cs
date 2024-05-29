namespace DatabaseInterfacing.Domain.DTOs;

public class MeasurementDto
{
    public int Id { get; set; }
    public int Time { get; set; }
    public float WaterTemperature { get; set; }
    public float WaterPH { get; set; }
    public int ElectricConductivity { get; set; }
    public int FlowRate { get; set; }
    public int WaterLevel { get; set; }
    public float AirTemperature { get; set; }
    public int AirHumidity { get; set; }
    public int CO2 { get; set; }
    public int VPD { get; set; }
    public int DewPoint { get; set; }
    public int LightLevels { get; set; }
}