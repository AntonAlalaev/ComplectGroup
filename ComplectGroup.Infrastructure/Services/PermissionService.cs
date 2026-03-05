using ComplectGroup.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace ComplectGroup.Infrastructure.Services;

/// <summary>
/// Сервис для работы с правами (permissions)
/// </summary>
public class PermissionService : IPermissionService
{
    private readonly UserManager<ApplicationUser> _userManager;

    // Описание всех прав
    private static readonly List<PermissionInfo> AllPermissions = new()
    {
        // Базовые права
        new PermissionInfo
        {
            Name = "CanView",
            DisplayName = "Просмотр данных",
            Description = "Доступ к просмотру всех данных системы",
            Category = "Базовые права"
        },
        new PermissionInfo
        {
            Name = "CanIgnoreComplectations",
            DisplayName = "Игнорирование комплектаций",
            Description = "Возможность помечать комплектации как игнорируемые",
            Category = "Базовые права"
        },

        // Складские операции
        new PermissionInfo
        {
            Name = "CanReceive",
            DisplayName = "Приходование на склад",
            Description = "Возможность оформлять приход товаров на склад",
            Category = "Склад"
        },
        new PermissionInfo
        {
            Name = "CanShip",
            DisplayName = "Отгрузка со склада",
            Description = "Возможность оформлять отгрузку товаров со склада",
            Category = "Склад"
        },
        new PermissionInfo
        {
            Name = "CanCorrect",
            DisplayName = "Корректировка пересортицы",
            Description = "Возможность оформлять корректировки пересортицы",
            Category = "Склад"
        },

        // Управление комплектациями
        new PermissionInfo
        {
            Name = "CanImportComplectations",
            DisplayName = "Загрузка из Excel",
            Description = "Возможность загружать комплектации из Excel файлов",
            Category = "Комплектации"
        },
        new PermissionInfo
        {
            Name = "CanEditComplectations",
            DisplayName = "Редактирование комплектаций",
            Description = "Возможность создавать и редактировать комплектации",
            Category = "Комплектации"
        },
        new PermissionInfo
        {
            Name = "CanDeleteComplectations",
            DisplayName = "Удаление комплектаций",
            Description = "Возможность удалять комплектации",
            Category = "Комплектации"
        },

        // Справочники
        new PermissionInfo
        {
            Name = "CanManageParts",
            DisplayName = "Управление деталями",
            Description = "Возможность создавать, редактировать и удалять детали",
            Category = "Справочники"
        },
        new PermissionInfo
        {
            Name = "CanManageChapters",
            DisplayName = "Управление разделами",
            Description = "Возможность создавать, редактировать и удалять разделы",
            Category = "Справочники"
        }
    };

    // Права по умолчанию для ролей
    private static readonly Dictionary<string, List<string>> RolePermissions = new()
    {
        ["Guest"] = new() { "CanView", "CanIgnoreComplectations" },
        ["Operator"] = new() { "CanView", "CanIgnoreComplectations" },
        ["SeniorOperator"] = new() { "CanView", "CanIgnoreComplectations", "CanReceive", "CanShip", "CanCorrect" },
        ["Manager"] = new() { "CanView", "CanIgnoreComplectations", "CanReceive", "CanShip", "CanCorrect", "CanImportComplectations", "CanEditComplectations", "CanDeleteComplectations" },
        ["Administrator"] = new() { "CanView", "CanIgnoreComplectations", "CanReceive", "CanShip", "CanCorrect", "CanImportComplectations", "CanEditComplectations", "CanDeleteComplectations", "CanManageParts", "CanManageChapters" }
    };

    public PermissionService(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public Task<List<PermissionInfo>> GetAllPermissionsAsync()
        => Task.FromResult(AllPermissions);

    public async Task<List<string>> GetUserPermissionsAsync(ApplicationUser user, CancellationToken ct = default)
    {
        var claims = await _userManager.GetClaimsAsync(user);
        return claims
            .Where(c => c.Type == "Permission")
            .Select(c => c.Value)
            .Distinct()
            .ToList();
    }

    public async Task SetUserPermissionsAsync(ApplicationUser user, List<string> permissions, CancellationToken ct = default)
    {
        // Получаем текущие Permission claims
        var currentClaims = await _userManager.GetClaimsAsync(user);
        var permissionClaims = currentClaims.Where(c => c.Type == "Permission").ToList();

        // Находим права для удаления
        var toRemove = permissionClaims.Where(c => !permissions.Contains(c.Value)).ToList();
        foreach (var claim in toRemove)
        {
            await _userManager.RemoveClaimAsync(user, claim);
        }

        // Находим права для добавления
        var toAdd = permissions
            .Where(p => !permissionClaims.Any(c => c.Value == p))
            .Select(p => new Claim("Permission", p))
            .ToList();
        foreach (var claim in toAdd)
        {
            await _userManager.AddClaimAsync(user, claim);
        }
    }

    public List<string> GetDefaultPermissionsForRole(string roleName)
        => RolePermissions.TryGetValue(roleName, out var perms) ? perms : new List<string>();

    public PermissionInfo? GetPermissionInfo(string permissionName)
        => AllPermissions.FirstOrDefault(p => p.Name == permissionName);
}
