using Auth.ServiceInterfaces;
using DatabaseInterfacing;
using DatabaseInterfacing.Context;
using DatabaseInterfacing.Domain.DTOs;
using DatabaseInterfacing.Domain.EntityFramework;

namespace Auth.Services;

public class UserAuthService : IUserAuthService
{
    public async Task<User> ValidateUser(UserValidationDto dto)
    {
        await using var dbContext = new PlantDbContext(DatabaseUtils.BuildConnectionOptions());
        
        
    }
}