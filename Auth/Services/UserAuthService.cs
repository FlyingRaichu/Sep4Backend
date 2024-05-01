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
        
        var query = dbContext.Users.AsQueryable();

        if (string.IsNullOrEmpty(dto.Username) || string.IsNullOrEmpty(dto.Password))
        {
            throw new Exception("Username or password field cannot be empty");
        }

        //Checks database for a user with the username
        var user = query.FirstOrDefault(user => user.UserName.Equals(dto.Username));
        
        //Returns the user if they're found and the password matches, otherwise throws
        return user != null && user.Password.Equals(dto.Password) ? user : throw new Exception("User not found in database.");
    }
}