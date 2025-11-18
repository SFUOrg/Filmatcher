using Filmatch.Models;
using Microsoft.AspNetCore.Identity;

namespace Filmatch;

public static class RoleInitializer
{
    public static async Task InitializeAsync(RoleManager<IdentityRole> roleManager)
    {
        if (await roleManager.FindByNameAsync(nameof(RoleEnum.Admin)) == null)
        {
            await roleManager.CreateAsync(new IdentityRole(nameof(RoleEnum.Admin)));
        }

        if (await roleManager.FindByNameAsync(nameof(RoleEnum.User)) == null)
        {
            await roleManager.CreateAsync(new IdentityRole(nameof(RoleEnum.User)));
        }
    }
}