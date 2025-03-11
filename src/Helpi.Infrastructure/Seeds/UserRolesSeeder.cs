using Helpi.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;

namespace Helpi.Infrastructure.Seeds;

public class RoleSeeder
{
    private readonly RoleManager<IdentityRole<int>> _roleManager;

    public RoleSeeder(RoleManager<IdentityRole<int>> roleManager)
    {
        _roleManager = roleManager;
    }

    public async Task SeedRolesAsync()
    {

        if (!await _roleManager.RoleExistsAsync(UserType.Admin.ToString()))
        {
            await _roleManager.CreateAsync(new IdentityRole<int>(UserType.Admin.ToString()));
        }


        if (!await _roleManager.RoleExistsAsync(UserType.Student.ToString()))
        {
            await _roleManager.CreateAsync(new IdentityRole<int>(UserType.Student.ToString()));
        }

        if (!await _roleManager.RoleExistsAsync(UserType.Customer.ToString()))
        {
            await _roleManager.CreateAsync(new IdentityRole<int>(UserType.Customer.ToString()));
        }

        // Add more roles as needed
    }
}