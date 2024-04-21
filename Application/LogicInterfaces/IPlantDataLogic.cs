using DatabaseInterfacing.Domain.EntityFramework;

namespace Application.LogicInterfaces;

public interface IPlantDataLogic
{
    //TODO Insert Search parameter DTO in GetAsync
    Task<IEnumerable<PlantData>> GetAsync();
}