using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DatabaseInterfacing.Domain.EntityFramework;

[Table("plant_data")]
public class PlantData
{
    [Key]
    public int Id { get; set; }

    [Required]
    [Column("plant_name")]
    public string PlantName { get; set; }

    [Column("water_temperature")]
    public float WaterTemperature { get; set; }
    
    [Column("ph_level")]
    public float PhLevel { get; set; }
    
    [Column("water_ec")]
    public float WaterEC { get; set; }
    
    [Column("water_flow")]
    public float WaterFlow { get; set; }
    [Column("date-time")]
    public DateTime DateTime { get; set; }
}