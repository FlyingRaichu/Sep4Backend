namespace DatabaseInterfacing.Domain.DTOs;

public class ThresholdConfigurationDto
{
    public double MinTemperature { get; set; }
    public double WarningTemperatureMin { get; set; }
    public double WarningTemperatureMax { get; set; }
    public double MaxTemperature { get; set; }
}