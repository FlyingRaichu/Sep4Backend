using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DatabaseInterfacing.Domain.EntityFramework;

[Table("alert_notifications")]
public class AlertNotification
{
    [Key]
    [Column("id")]
    public int Id { get; set; }
    
    [Column("parameter_type")]
    public string ParameterType { get; set; }
    
    [Column("threshold_min")]
    public double ThresholdMin { get; set; }
    
    [Column("threshold_max")]
    public double ThresholdMax { get; set; }
    
    [Column("warning_min")]
    public double WarningMin { get; set; }
    
    [Column("warning_max")]
    public double WarningMax { get; set; }

    [Required]
    [EmailAddress]
    [Column("email")]
    public string Email { get; set; }
}