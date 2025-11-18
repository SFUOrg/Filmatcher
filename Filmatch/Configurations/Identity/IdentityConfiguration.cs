using System.Text;
using Filmatch.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace Filmatch.Configurations.Identity;

public static class IdentityConfiguration
{
    public static IServiceCollection ConfigureIdentity(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddIdentity<IdentityUser, IdentityRole>()
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();
        
        return services;
    }
}