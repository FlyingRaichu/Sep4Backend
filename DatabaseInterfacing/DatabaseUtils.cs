//This class is used for any Database access utilities

using DatabaseInterfacing.Context;
using Microsoft.EntityFrameworkCore;

namespace DatabaseInterfacing;

public static class DatabaseUtils
{
    private const string ConnectionString = "Host=sep4.postgres.database.azure.com;Port=5432;Database=postgres;Username=BigChungus;Password=f):4)MYfZMUh3ww;SSL Mode=Require;Trust Server Certificate=true;";

    public static DbContextOptions<PlantDbContext> BuildConnectionOptions()
    {
        var optionsBuilder = new DbContextOptionsBuilder<PlantDbContext>();
        optionsBuilder.UseNpgsql(ConnectionString);

        return optionsBuilder.Options;
    }
}