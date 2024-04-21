using Application.LogicInterfaces;
using DatabaseInterfacing;
using DatabaseInterfacing.Context;
using DatabaseInterfacing.Domain.EntityFramework;
using Microsoft.EntityFrameworkCore;

namespace Application.Logic;

public class PlantDataLogic : IPlantDataLogic
{
    public async Task<IEnumerable<PlantData>> GetAsync()
    {
        await using var dbContext = new PlantDbContext(DatabaseUtils.BuildConnectionOptions());
        return await dbContext.PlantData.ToListAsync();
    }
}