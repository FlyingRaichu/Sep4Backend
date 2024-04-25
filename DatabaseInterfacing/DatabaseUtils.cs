//This class is used for any Database access utilities

using DatabaseInterfacing.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace DatabaseInterfacing;

public class DatabaseUtils : IDesignTimeDbContextFactory<PlantDbContext>
{
    private const string ConnectionString = "Host=lucky.db.elephantsql.com;Port=5432;Database=odvlulua;Username=odvlulua;Password=oBlgMTohINy3iW30-RxXfKY0GyrtS1c2;SSL Mode=Require;Trust Server Certificate=true;";

    public static DbContextOptions<PlantDbContext> BuildConnectionOptions()
    {
        var optionsBuilder = new DbContextOptionsBuilder<PlantDbContext>();
        optionsBuilder.UseNpgsql(ConnectionString);

        return optionsBuilder.Options;
    }

    
    //This is for Migrations, no touchie
    public PlantDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<PlantDbContext>();
        optionsBuilder.UseNpgsql(ConnectionString);

        return new PlantDbContext(optionsBuilder.Options);
    }
}