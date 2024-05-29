namespace DatabaseInterfacing.Domain.DTOs;

public class AlertNotificationDto
{
    public int Id { get; set; }
    public string ParameterType { get; set; }
    public double ThresholdMin { get; set; }
    public double ThresholdMax { get; set; }
    public string Email { get; set; }
}