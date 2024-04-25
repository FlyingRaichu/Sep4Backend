//This class is used for any Database access utilities

using DatabaseInterfacing.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace DatabaseInterfacing;

public abstract class DatabaseUtils : IDesignTimeDbContextFactory<PlantDbContext>
{
    private static readonly IConfiguration configuration = new ConfigurationBuilder()
        .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
        .Build();

    public static DbContextOptions<PlantDbContext> BuildConnectionOptions()
    {
        var optionsBuilder = new DbContextOptionsBuilder<PlantDbContext>();
        optionsBuilder.UseNpgsql(GetConnectionString());

        return optionsBuilder.Options;
    }

    private static string GetConnectionString()
    {
        return configuration.GetConnectionString("DefaultConnection") ??
               "Host=lucky.db.elephantsql.com;Port=5432;Database=odvlulua;Username=odvlulua;Password=oBlgMTohINy3iW30-RxXfKY0GyrtS1c2;SSL Mode=Require;Trust Server Certificate=true;";
    }


    //This is for Migrations, no touchie
    public PlantDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<PlantDbContext>();
        optionsBuilder.UseNpgsql(GetConnectionString());

        return new PlantDbContext(optionsBuilder.Options);
    }
}