using Microsoft.AspNetCore.Identity;

namespace collectiontracker.Data
{
    public class SeedData
    {
        public static async Task Initialize(IConfiguration configuration, UserManager<IdentityUser<int>> userManager, RoleManager<IdentityRole<int>> roleManager)
        {
            await SeedRoles(roleManager);

            await SeedAdminUser(configuration, userManager);

            await SeedRegularUser(configuration, userManager);
        }

        private static async Task SeedRoles(RoleManager<IdentityRole<int>> roleManager)
        {
            if (!await roleManager.RoleExistsAsync("Admin"))
            {
                await roleManager.CreateAsync(new IdentityRole<int> { Name = "Admin" });
            }


            if (!await roleManager.RoleExistsAsync("Regular"))
            {
                await roleManager.CreateAsync(new IdentityRole<int> { Name = "Regular" });
            }
        }

        private static async Task SeedAdminUser(IConfiguration configuration, UserManager<IdentityUser<int>> userManager)
        {
            var adminEmail = configuration["ADMIN_EMAIL"];
            var adminPassword = configuration["ADMIN_PASSWORD"];

            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                var newUser = new IdentityUser<int> { UserName = adminEmail, Email = adminEmail };
                var result = await userManager.CreateAsync(newUser, adminPassword);

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(newUser, "Admin");
                }
            }
        }

        private static async Task SeedRegularUser(IConfiguration configuration, UserManager<IdentityUser<int>> userManager)
        {
            var regularEmail = configuration["REGULAR_USER_EMAIL"];
            var regularPassword = configuration["REGULAR_USER_PASSWORD"];

            var regularUser = await userManager.FindByEmailAsync(regularEmail);

            if (regularUser != null)
            {
                Console.WriteLine($"A user with the email {regularEmail} already exists");
                return;
            }

            var newUser = new IdentityUser<int>
            {
                UserName = regularEmail,
                Email = regularEmail
            };

            var result = await userManager.CreateAsync(newUser, regularPassword);

            if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(newUser, "Regular");
                }
            }
        }
    }
