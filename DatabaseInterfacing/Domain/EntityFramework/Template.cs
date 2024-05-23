using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DatabaseInterfacing.Domain.EntityFramework
{
    [Table("template")]
    public class Template
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Column("name")]
        public string Name { get; set; }

        // Navigation property for one-to-many relationship with Parameter
        public virtual ICollection<Parameter> Parameters { get; set; }

        public Template()
        {
            Parameters = new HashSet<Parameter>();
        }
    }
}