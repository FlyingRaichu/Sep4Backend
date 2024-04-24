using Microsoft.Extensions.DependencyInjection;

namespace Auth;

public static class AuthorizationPolicies
{
    public static void AddPolicies(IServiceCollection services)
    {
        services.AddAuthorizationCore(options =>
        {
            options.AddPolicy("MustBeLoggedIn", policy => policy.RequireAuthenticatedUser());
        });
    }
}