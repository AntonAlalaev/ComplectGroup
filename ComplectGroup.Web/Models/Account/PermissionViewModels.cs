using System.ComponentModel.DataAnnotations;

namespace ComplectGroup.Web.Models.Account;

/// <summary>
/// Модель для отображения прав (permissions) в UI
/// </summary>
public class PermissionViewModel
{
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public bool IsAssigned { get; set; }
}

/// <summary>
/// Модель для редактирования прав пользователя
/// </summary>
public class EditUserPermissionsViewModel
{
    public string UserId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;

    /// <summary>
    /// Все доступные права, сгруппированные по категориям
    /// </summary>
    public Dictionary<string, List<PermissionViewModel>> PermissionsByCategory { get; set; } = new();

    /// <summary>
    /// Выбранные права (для отправки на сервер)
    /// </summary>
    public List<string> SelectedPermissions { get; set; } = new();
}

/// <summary>
/// Модель для создания/редактирования пользователя с правами
/// </summary>
public class UserWithPermissionsViewModel
{
    public string Id { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string FullName { get; set; } = string.Empty;

    [StringLength(100, MinimumLength = 6)]
    public string? Password { get; set; }

    [Compare(nameof(Password))]
    public string? ConfirmPassword { get; set; }

    /// <summary>
    /// Выбранная роль
    /// </summary>
    public string SelectedRole { get; set; } = string.Empty;

    /// <summary>
    /// Доступные роли для выбора
    /// </summary>
    public List<string> AvailableRoles { get; set; } = new();

    /// <summary>
    /// Выбранные права (переопределяют права роли)
    /// </summary>
    public List<string> SelectedPermissions { get; set; } = new();

    /// <summary>
    /// Все доступные права для отображения
    /// </summary>
    public Dictionary<string, List<PermissionViewModel>> PermissionsByCategory { get; set; } = new();

    public bool IsActive { get; set; } = true;
}
