using DatabaseInterfacing.Domain.DTOs;
using DatabaseInterfacing.Domain.EntityFramework;

namespace Auth.ServiceInterfaces;

public interface IUserAuthService
{
    Task<User> ValidateUser(UserValidationDto dto);
}