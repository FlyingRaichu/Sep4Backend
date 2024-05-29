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

    [Column("waterConductivity")]
    public float? WaterConductivity { get; set; }

    [Column("waterTemperature")]
    public float? WaterTemperature { get; set; }

    [Column("waterPh")]
    public float? WaterPhLevel { get; set; }

    [Column("waterFlow")] 
    public float? WaterFlow { get; set; }

    [Column("waterLevel")] 
    public float? WaterLevel { get; set; }

    [Column("airTemperature")] 
    public float? AirTemperature { get; set; }

    [Column("airHumidity")] 
    public float? AirHumidity { get; set; }
        
    [Column("airCo2")]
    public float? AirCO2 { get; set; }

    [Column("lightLevel")] 
    public float? LightLevel { get; set; }
    
    [Column("dewPoint")] 
    public float? DewPoint { get; set; }
    
    [Column("vpdLevel")] 
    public float? VpdLevel { get; set; }
    
    [Column("date-time")]
    public DateTime DateTime { get; set; }
}