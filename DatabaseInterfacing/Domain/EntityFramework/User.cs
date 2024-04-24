using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DatabaseInterfacing.Domain.EntityFramework;


[Table("users")]
public class User
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    [Column("username")]
    public required string UserName { get; set; }
    
    [Column("password")]
    public required string Password { get; set; }
    
    [Column("email")]
    public required string Email { get; set; }
}