using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ComplectGroup.Infrastructure.Identity; // или ComplectGroup.Domain.Entities
using System;
using System.Threading.Tasks;

namespace ComplectGroup.Infrastructure.Data
{
    public static class SeedData
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();

            // Проверяем, есть ли уже данные в базе
            if (await context.Users.AnyAsync())
            {
                Console.WriteLine("⚠️ Database already has data. Skipping seed.");
                return;
            }

            Console.WriteLine("🌱 Seeding identity data...");

            // 1. Создание ролей
            string[] roleNames = { "Administrator", "Manager", "User" };
            
            foreach (var roleName in roleNames)
            {
                var roleExist = await roleManager.RoleExistsAsync(roleName);
                if (!roleExist)
                {
                    await roleManager.CreateAsync(new ApplicationRole 
                    { 
                        Name = roleName, 
                        Description = $"{roleName} role",
                        CreatedAt = DateTime.UtcNow
                    });
                    Console.WriteLine($"✅ Created role: {roleName}");
                }
                else
                {
                    Console.WriteLine($"⚠️ Role already exists: {roleName}");
                }
            }

            // 2. Создание администратора
            var adminEmail = "admin@complectgroup.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            
            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FullName = "System Administrator",
                    EmailConfirmed = true,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };

                var result = await userManager.CreateAsync(adminUser, "Admin123!");
                
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Administrator");
                    Console.WriteLine($"✅ Created admin user: {adminEmail}");
                }
                else
                {
                    Console.WriteLine($"❌ Failed to create admin user: {string.Join(", ", result.Errors)}");
                }
            }
            else
            {
                Console.WriteLine($"⚠️ Admin user already exists: {adminEmail}");
            }

            // 3. Создание тестового менеджера (опционально)
            var managerEmail = "manager@complectgroup.com";
            var managerUser = await userManager.FindByEmailAsync(managerEmail);
            
            if (managerUser == null)
            {
                managerUser = new ApplicationUser
                {
                    UserName = managerEmail,
                    Email = managerEmail,
                    FullName = "Test Manager",
                    EmailConfirmed = true,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };

                var result = await userManager.CreateAsync(managerUser, "Manager123!");
                
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(managerUser, "Manager");
                    Console.WriteLine($"✅ Created manager user: {managerEmail}");
                }
            }

            // 4. Создание тестового обычного пользователя (опционально)
            var userEmail = "user@complectgroup.com";
            var testUser = await userManager.FindByEmailAsync(userEmail);
            
            if (testUser == null)
            {
                testUser = new ApplicationUser
                {
                    UserName = userEmail,
                    Email = userEmail,
                    FullName = "Test User",
                    EmailConfirmed = true,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };

                var result = await userManager.CreateAsync(testUser, "User123!");
                
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(testUser, "User");
                    Console.WriteLine($"✅ Created test user: {userEmail}");
                }
            }

            Console.WriteLine("✅ Identity data seeding completed!");
        }
    }
}