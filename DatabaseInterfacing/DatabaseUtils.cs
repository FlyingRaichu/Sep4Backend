//This class is used for any Database access utilities

using DatabaseInterfacing.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace DatabaseInterfacing;

public abstract class DatabaseUtils : IDesignTimeDbContextFactory<PlantDbContext>
{
    //This creates a configuration based on the appsettings.json in this particular project 
    private static readonly IConfiguration Configuration = new ConfigurationBuilder()
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .Build();

    //Use this anytime you need to create a database context option
    public static DbContextOptions<PlantDbContext> BuildConnectionOptions()
    {
        var optionsBuilder = new DbContextOptionsBuilder<PlantDbContext>();
        optionsBuilder.UseNpgsql(GetConnectionString());

        return optionsBuilder.Options;
    }

    //Pulls the connection string from the local appsettings.json
    private static string GetConnectionString()
    {
        return Configuration.GetConnectionString("DefaultConnection") ??
               throw new Exception(
                   "Your connection string is not configured correctly in WebAPI/appsettings.json, or doesn't exist.");
    }

    //This is for Migrations, no touchie
    public PlantDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<PlantDbContext>();
        optionsBuilder.UseNpgsql(GetConnectionString());

        return new PlantDbContext(optionsBuilder.Options);
    }
}