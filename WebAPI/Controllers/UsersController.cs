using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Auth.ServiceInterfaces;
using DatabaseInterfacing.Domain.DTOs;
using DatabaseInterfacing.Domain.EntityFramework;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace Sep4Backend.Controllers;

[ApiController]
[Route("[controller]")]
public class UsersController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly IUserAuthService _authService;

    public UsersController(IConfiguration configuration, IUserAuthService authService)
    {
        _configuration = configuration;
        _authService = authService;
    }


    //Generate claims. Any info put here will be in the final JWT token and will be transferable to the front-end
    private IEnumerable<Claim> GenerateClaims(User user)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub,
                _configuration["Jwt:Subject"] ??
                throw new Exception("Could not find settings for Subject in appsettings.json")),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Iat, DateTime.UtcNow.ToString(CultureInfo.InvariantCulture)),
            new Claim(ClaimTypes.Name, user.UserName)
        };
        return claims.ToList();
    }

    //Magic code that creates the JWT using the claims from GenerateClaims
    private string GenerateJwt(User user)
    {
        var claims = GenerateClaims(user);

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ??
                                                                  throw new Exception(
                                                                      "Could not find settings for Key in appsettings.json")));
        var signIn = new SigningCredentials(key, SecurityAlgorithms.HmacSha512);

        var header = new JwtHeader(signIn);

        var payload = new JwtPayload(
            _configuration["Jwt:Issuer"],
            _configuration["Jwt:Audience"],
            claims,
            null,
            DateTime.UtcNow.AddMinutes(60));

        var token = new JwtSecurityToken(header, payload);

        var serializedToken = new JwtSecurityTokenHandler().WriteToken(token);
        return serializedToken;
    }

    [HttpPost, Route("login")]
    public async Task<ActionResult> Login([FromBody] UserValidationDto dto)
    {
        try
        {
            var user = await _authService.ValidateUser(dto);
            var token = GenerateJwt(user);

            return Ok(token);
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }
}