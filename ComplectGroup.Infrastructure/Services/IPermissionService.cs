using ComplectGroup.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace ComplectGroup.Infrastructure.Services;

/// <summary>
/// Сервис для работы с правами (permissions)
/// </summary>
public interface IPermissionService
{
    /// <summary>
    /// Получить все доступные права с описанием
    /// </summary>
    Task<List<PermissionInfo>> GetAllPermissionsAsync();

    /// <summary>
    /// Получить права пользователя
    /// </summary>
    Task<List<string>> GetUserPermissionsAsync(ApplicationUser user, CancellationToken ct = default);

    /// <summary>
    /// Назначить права пользователю
    /// </summary>
    Task SetUserPermissionsAsync(ApplicationUser user, List<string> permissions, CancellationToken ct = default);

    /// <summary>
    /// Получить права по умолчанию для роли
    /// </summary>
    List<string> GetDefaultPermissionsForRole(string roleName);

    /// <summary>
    /// Получить описание права
    /// </summary>
    PermissionInfo? GetPermissionInfo(string permissionName);
}

/// <summary>
/// Информация о праве (permission)
/// </summary>
public class PermissionInfo
{
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
}
