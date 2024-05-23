namespace DatabaseInterfacing.Domain.EntityFramework;

public class Parameter
{
    public int Id { get; set; }
    public int TemplateId { get; set; }
    public string Type { get; set; }
    public double Min { get; set; }
    public double WarningMin { get; set; }
    public double WarningMax { get; set; }
    public double Max { get; set; }    
}