using DatabaseInterfacing.Domain.EntityFramework;
using Microsoft.EntityFrameworkCore;

namespace DatabaseInterfacing.Context;

public class PlantDbContext : DbContext
{
    public PlantDbContext(DbContextOptions<PlantDbContext> options) : base(options)
    {
    }
    
    public DbSet<PlantData> PlantData { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Template> Templates { get; set; }
    public DbSet<Parameter> Parameters { get; set; }
    // public DbSet<Measurement> Measurements { get; set; }
    public DbSet<AlertNotification> AlertNotifications { get; set; }
}