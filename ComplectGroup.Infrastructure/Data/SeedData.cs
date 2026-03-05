using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ComplectGroup.Infrastructure.Identity; // или ComplectGroup.Domain.Entities
using ComplectGroup.Infrastructure.Services;
using System;
using System.Threading.Tasks;

namespace ComplectGroup.Infrastructure.Data
{
    public static class SeedData
    {
        // ===== Список всех возможных прав (permissions) =====
        public static class Permissions
        {
            public const string CanView = "CanView";
            public const string CanIgnoreComplectations = "CanIgnoreComplectations";
            public const string CanReceive = "CanReceive";
            public const string CanShip = "CanShip";
            public const string CanCorrect = "CanCorrect";
            public const string CanImportComplectations = "CanImportComplectations";
            public const string CanEditComplectations = "CanEditComplectations";
            public const string CanDeleteComplectations = "CanDeleteComplectations";
            public const string CanManageParts = "CanManageParts";
            public const string CanManageChapters = "CanManageChapters";
        }

        // ===== Права по умолчанию для каждой роли =====
        public static readonly Dictionary<string, List<string>> RolePermissions = new()
        {
            // Guest - только просмотр + игнорирование
            ["Guest"] = new() { Permissions.CanView, Permissions.CanIgnoreComplectations },

            // Operator - склад: приход ИЛИ отгрузка (настраивается индивидуально)
            ["Operator"] = new() { Permissions.CanView, Permissions.CanIgnoreComplectations },

            // SeniorOperator - склад + коррективы
            ["SeniorOperator"] = new()
            {
                Permissions.CanView,
                Permissions.CanIgnoreComplectations,
                Permissions.CanReceive,
                Permissions.CanShip,
                Permissions.CanCorrect
            },

            // Manager - всё кроме управления справочниками
            ["Manager"] = new()
            {
                Permissions.CanView,
                Permissions.CanIgnoreComplectations,
                Permissions.CanReceive,
                Permissions.CanShip,
                Permissions.CanCorrect,
                Permissions.CanImportComplectations,
                Permissions.CanEditComplectations,
                Permissions.CanDeleteComplectations
            },

            // Administrator - полный доступ
            ["Administrator"] = new()
            {
                Permissions.CanView,
                Permissions.CanIgnoreComplectations,
                Permissions.CanReceive,
                Permissions.CanShip,
                Permissions.CanCorrect,
                Permissions.CanImportComplectations,
                Permissions.CanEditComplectations,
                Permissions.CanDeleteComplectations,
                Permissions.CanManageParts,
                Permissions.CanManageChapters
            }
        };

        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
            var userManagementService = scope.ServiceProvider.GetRequiredService<IUserManagementService>();

            // Проверяем, есть ли уже данные в базе
            if (await context.Users.AnyAsync())
            {
                Console.WriteLine("⚠️ Database already has data. Skipping seed.");
                return;
            }

            Console.WriteLine("🌱 Seeding identity data...");

            // 1. Создание ролей
            string[] roleNames = { "Administrator", "Manager", "SeniorOperator", "Operator", "Guest" };

            foreach (var roleName in roleNames)
            {
                var roleExist = await roleManager.RoleExistsAsync(roleName);
                if (!roleExist)
                {
                    await roleManager.CreateAsync(new ApplicationRole
                    {
                        Name = roleName,
                        Description = GetRoleDescription(roleName),
                        CreatedAt = DateTime.UtcNow
                    });
                    Console.WriteLine($"✅ Created role: {roleName}");
                }
                else
                {
                    Console.WriteLine($"⚠️ Role already exists: {roleName}");
                }
            }

            // 2. Создание администратора с полными правами
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
                    // Назначаем все права администратора
                    await userManagementService.SetUserPermissionsAsync(adminUser, RolePermissions["Administrator"], default);
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

            // 3. Создание тестового менеджера
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
                    await userManagementService.SetUserPermissionsAsync(managerUser, RolePermissions["Manager"], default);
                    Console.WriteLine($"✅ Created manager user: {managerEmail}");
                }
            }

            // 4. Создание старшего оператора (начальник склада)
            var seniorOperatorEmail = "senior@complectgroup.com";
            var seniorOperatorUser = await userManager.FindByEmailAsync(seniorOperatorEmail);

            if (seniorOperatorUser == null)
            {
                seniorOperatorUser = new ApplicationUser
                {
                    UserName = seniorOperatorEmail,
                    Email = seniorOperatorEmail,
                    FullName = "Senior Operator",
                    EmailConfirmed = true,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };

                var result = await userManager.CreateAsync(seniorOperatorUser, "Senior123!");

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(seniorOperatorUser, "SeniorOperator");
                    await userManagementService.SetUserPermissionsAsync(seniorOperatorUser, RolePermissions["SeniorOperator"], default);
                    Console.WriteLine($"✅ Created senior operator user: {seniorOperatorEmail}");
                }
            }

            // 5. Создание оператора склада (только просмотр, права на приход/отгрузку назначаются индивидуально)
            var operatorEmail = "operator@complectgroup.com";
            var operatorUser = await userManager.FindByEmailAsync(operatorEmail);

            if (operatorUser == null)
            {
                operatorUser = new ApplicationUser
                {
                    UserName = operatorEmail,
                    Email = operatorEmail,
                    FullName = "Warehouse Operator",
                    EmailConfirmed = true,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };

                var result = await userManager.CreateAsync(operatorUser, "Operator123!");

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(operatorUser, "Operator");
                    // Оператору по умолчанию даём только просмотр, права на приход/отгрузку назначаются индивидуально
                    await userManagementService.SetUserPermissionsAsync(operatorUser, RolePermissions["Operator"], default);
                    Console.WriteLine($"✅ Created operator user: {operatorEmail}");
                }
            }

            // 6. Создание тестового пользователя (Guest - только просмотр)
            var guestEmail = "guest@complectgroup.com";
            var guestUser = await userManager.FindByEmailAsync(guestEmail);

            if (guestUser == null)
            {
                guestUser = new ApplicationUser
                {
                    UserName = guestEmail,
                    Email = guestEmail,
                    FullName = "Guest User",
                    EmailConfirmed = true,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };

                var result = await userManager.CreateAsync(guestUser, "Guest123!");

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(guestUser, "Guest");
                    await userManagementService.SetUserPermissionsAsync(guestUser, RolePermissions["Guest"], default);
                    Console.WriteLine($"✅ Created guest user: {guestEmail}");
                }
            }

            Console.WriteLine("✅ Identity data seeding completed!");
        }

        private static string GetRoleDescription(string roleName) => roleName switch
        {
            "Administrator" => "Полный доступ ко всем функциям системы",
            "Manager" => "Управление комплектациями, загрузка из Excel, складские операции",
            "SeniorOperator" => "Старший оператор склада: приход, отгрузка, коррективы",
            "Operator" => "Оператор склада: базовые операции (требуются дополнительные права)",
            "Guest" => "Только просмотр данных + игнорирование комплектаций",
            _ => $"{roleName} role"
        };
    }
}