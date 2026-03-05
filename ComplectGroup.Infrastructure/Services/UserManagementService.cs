// ComplectGroup.Infrastructure/Services/UserManagementService.cs
using ComplectGroup.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ComplectGroup.Infrastructure.Services;

/// <summary>
/// Сервис управления пользователями
/// включает в себя методы для работы с пользователями и ролями:
///     - получение списка пользователей
///     - обновление пользователя
///     - создание пользователя
///     - удаление пользователя
///     - получение списка ролей
///     - обновление ролей пользователя
///     - создание роли
///     - удаление роли
/// </summary>
public class UserManagementService : IUserManagementService
{
    /// <summary>
    /// UserManager для работы с пользователями
    /// </summary>
    private readonly UserManager<ApplicationUser> _userManager;
    /// <summary>
    /// RoleManager для работы с ролями
    /// </summary>
    private readonly RoleManager<ApplicationRole> _roleManager;  // ← ApplicationRole
    private readonly ILogger<UserManagementService> _logger;

    /// <summary>
    /// Конструктор сервиса управления пользователями
    /// </summary>
    /// <param name="userManager"></param>
    /// <param name="roleManager"></param>
    /// <param name="logger"></param>
    public UserManagementService(
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager,  // ← ApplicationRole
        ILogger<UserManagementService> logger)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _logger = logger;
    }

    /// <summary>
    /// Возвращает список всех пользователей
    /// </summary>
    public async Task<List<ApplicationUser>> GetAllUsersAsync(CancellationToken ct) =>
        await _userManager.Users.OrderBy(u => u.Email).ToListAsync(ct);

    /// <summary>
    /// Возвращает пользователя по его ID
    /// </summary>
    public async Task<ApplicationUser?> GetUserByIdAsync(Guid id, CancellationToken ct) =>
        await _userManager.FindByIdAsync(id.ToString());

    /// <summary>
    /// Обновляет пользователя по его ID
    /// </summary>
    public async Task<IdentityResult> UpdateUserAsync(Guid id, string email, string fullName, bool isActive, CancellationToken ct)
    {
        var user = await GetUserByIdAsync(id, ct) ?? throw new KeyNotFoundException("Пользователь не найден");
        
        user.Email = email;
        user.FullName = fullName;
        user.IsActive = isActive;
        user.NormalizedEmail = email.ToUpper();
        
        return await _userManager.UpdateAsync(user);
    }

    /// <summary>
    /// Обновляет роли пользователя по его ID
    /// </summary>
    public async Task<IdentityResult> UpdateUserRolesAsync(Guid userId, List<string> newRoles, CancellationToken ct)
    {
        var user = await GetUserByIdAsync(userId, ct) ?? throw new KeyNotFoundException("Пользователь не найден");
        
        var currentRoles = await _userManager.GetRolesAsync(user);
        
        foreach (var role in currentRoles.Except(newRoles))
        {
            await _userManager.RemoveFromRoleAsync(user, role);
        }
        
        foreach (var role in newRoles.Except(currentRoles))
        {
            await _userManager.AddToRoleAsync(user, role);
        }
        
        return IdentityResult.Success;
    }

    /// <summary>
    /// Создает пользователя
    /// </summary>
    /// <param name="email"></param>
    /// <param name="fullName"></param>
    /// <param name="password"></param>
    /// <param name="roles"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    public async Task<IdentityResult> CreateUserAsync(string email, string fullName, string password, List<string> roles, CancellationToken ct)
    {
        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            FullName = fullName,
            IsActive = true,
            EmailConfirmed = true
        };
        
        var result = await _userManager.CreateAsync(user, password);
        
        if (result.Succeeded && roles.Any())
        {
            await _userManager.AddToRolesAsync(user, roles);
        }
        
        return result;
    }

    /// <summary>
    /// Удаляет пользователя по его ID
    /// </summary>
    /// <param name="id"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    /// <exception cref="KeyNotFoundException"></exception>
    public async Task<IdentityResult> DeleteUserAsync(Guid id, CancellationToken ct)
    {
        var user = await GetUserByIdAsync(id, ct) ?? throw new KeyNotFoundException("Пользователь не найден");
        
        if (user.Email == "admin@complectgroup.com")
            return IdentityResult.Failed(new IdentityError { Description = "Нельзя удалить главного администратора" });
        
        return await _userManager.DeleteAsync(user);
    }

    /// <summary>
    /// Возвращает список всех ролей
    /// </summary>
    public async Task<List<ApplicationRole>> GetAllRolesAsync(CancellationToken ct) =>  // ← ApplicationRole
        await _roleManager.Roles.ToListAsync(ct);
}