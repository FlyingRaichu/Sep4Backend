using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DatabaseInterfacing.Domain.EntityFramework;

[Table("PlantData")]
public class PlantData
{
    [Key]
    public int Id { get; set; }

    [Required]
    [Column("PlantName")]
    public string PlantName { get; set; }

    [Column("WaterTemperature")]
    public float WaterTemperature { get; set; }

    [Column("PHLevel")]
    public float PHLevel { get; set; }
}