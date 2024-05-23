using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DatabaseInterfacing.Domain.EntityFramework
{
    public class Parameter
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Template")]
        public int TemplateId { get; set; }
        public string Type { get; set; }
        public double Min { get; set; }
        public double WarningMin { get; set; }
        public double WarningMax { get; set; }
        public double Max { get; set; }
        
        public virtual Template Template { get; set; }
    }
}