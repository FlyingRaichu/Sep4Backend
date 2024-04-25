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
    public string UserName { get; set; }
    
    [Column("password")]
    public string Password { get; set; }
    
    [Column("email")]
    public string Email { get; set; }
}