// ComplectGroup.Web/Controllers/AccountController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using ComplectGroup.Infrastructure.Identity;
using ComplectGroup.Infrastructure.Services;
using ComplectGroup.Web.Models.Account;


namespace ComplectGroup.Web.Controllers;


/// <summary>
/// Контроллер для работы с аутентификацией и авторизацией
/// </summary>
[Authorize]
public class AccountController : Controller
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<AccountController> _logger;
    private readonly IUserManagementService _userManagementService;
    private readonly IPermissionService _permissionService;

    public AccountController(
        IUserManagementService userManagementService,
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        ILogger<AccountController> logger,
        IPermissionService permissionService)
    {
        _userManagementService = userManagementService;
        _userManager = userManager;
        _signInManager = signInManager;
        _logger = logger;
        _permissionService = permissionService;
    }

    // GET: /Account/Login
    /// <summary>
    /// Метод для отображения формы входа в систему
    /// </summary>
    /// <param name="returnUrl"></param>
    /// <returns></returns>
    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    // POST: /Account/Login
    /// <summary>
    /// Метод для обработки формы входа в систему
    /// </summary>
    /// <param name="model"></param>
    /// <param name="returnUrl"></param>
    /// <returns></returns>
    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;

        if (ModelState.IsValid)
        {
            var result = await _signInManager.PasswordSignInAsync(
                model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);

            if (result.Succeeded)
            {
                _logger.LogInformation("Пользователь {Email} вошел в систему", model.Email);
                return RedirectToLocal(returnUrl);
            }
            if (result.IsLockedOut)
            {
                _logger.LogWarning("Аккаунт пользователя {Email} заблокирован", model.Email);
                return View("Lockout");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Неверный email или пароль");
                return View(model);
            }
        }

        return View(model);
    }

    // GET: /Account/Register
    /// <summary>
    /// Метод для отображения формы регистрации пользователя
    /// </summary>
    /// <param name="returnUrl"></param>
    /// <returns></returns>
    [HttpGet]
    [AllowAnonymous]
    public IActionResult Register(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    // POST: /Account/Register
    /// <summary>
    /// Метод для обработки формы регистрации пользователя
    /// </summary>
    /// <param name="model"></param>
    /// <param name="returnUrl"></param>
    /// <returns></returns>
    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;

        if (ModelState.IsValid)
        {
            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FullName = model.FullName,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                _logger.LogInformation("Создан новый аккаунт с email {Email}", model.Email);
                await _userManager.AddToRoleAsync(user, "User");
                await _signInManager.SignInAsync(user, isPersistent: false);
                
                return RedirectToLocal(returnUrl);
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        return View(model);
    }

    // POST: /Account/Logout
    /// <summary>
    /// Метод для выхода из системы
    /// </summary>
    /// <returns></returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        _logger.LogInformation("Пользователь вышел из системы");
        return RedirectToAction("Index", "Home");
    }

    // GET: /Account/AccessDenied
    /// <summary>
    /// Метод для отображения страницы доступа запрещен
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public IActionResult AccessDenied()
    {
        return View();
    }

    // GET: /Account/Users
    /// <summary>
    /// Метод для отображения списка пользователей (только для администраторов)
    /// </summary>
    [Authorize(Policy = "RequireAdministrator")]
    public async Task<IActionResult> Users(CancellationToken cancellationToken)
    {
        var users = await _userManagementService.GetAllUsersAsync(cancellationToken);
        return View(users);
    }

    // GET: /Account/EditUser/{id}
    /// <summary>
    /// Редактирование пользователя и его прав (только для администраторов)
    /// </summary>
    [Authorize(Policy = "RequireAdministrator")]
    public async Task<IActionResult> EditUser(Guid id, CancellationToken cancellationToken)
    {
        var user = await _userManagementService.GetUserByIdAsync(id, cancellationToken);
        if (user == null) return NotFound();

        var allRoles = await _userManagementService.GetAllRolesAsync(cancellationToken);
        var userRoles = await _userManager.GetRolesAsync(user);
        var userPermissions = await _permissionService.GetUserPermissionsAsync(user, cancellationToken);
        var allPermissions = await _permissionService.GetAllPermissionsAsync();

        // Группируем права по категориям
        var permissionsByCategory = allPermissions
            .GroupBy(p => p.Category)
            .ToDictionary(
                g => g.Key,
                g => g.Select(p => new PermissionViewModel
                {
                    Name = p.Name,
                    DisplayName = p.DisplayName,
                    Description = p.Description,
                    Category = p.Category,
                    IsAssigned = userPermissions.Contains(p.Name)
                }).ToList()
            );

        var model = new UserWithPermissionsViewModel
        {
            Id = user.Id.ToString(),
            Email = user.Email ?? "",
            FullName = user.FullName ?? "",
            IsActive = user.IsActive,
            SelectedRole = userRoles.FirstOrDefault() ?? "",
            AvailableRoles = allRoles.Select(r => r.Name ?? "").ToList(),
            SelectedPermissions = userPermissions,
            PermissionsByCategory = permissionsByCategory
        };

        return View(model);
    }

    // POST: /Account/EditUser
    /// <summary>
    /// Сохранение изменений пользователя и его прав (только для администраторов)
    /// </summary>
    [Authorize(Policy = "RequireAdministrator")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditUser(UserWithPermissionsViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            // Восстанавливаем права для отображения
            var allPermissions = await _permissionService.GetAllPermissionsAsync();
            model.PermissionsByCategory = allPermissions
                .GroupBy(p => p.Category)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(p => new PermissionViewModel
                    {
                        Name = p.Name,
                        DisplayName = p.DisplayName,
                        Description = p.Description,
                        Category = p.Category,
                        IsAssigned = model.SelectedPermissions.Contains(p.Name)
                    }).ToList()
                );
            return View(model);
        }

        try
        {
            var userId = Guid.Parse(model.Id);
            var user = await _userManagementService.GetUserByIdAsync(userId, cancellationToken);
            if (user == null) return NotFound();

            // Обновляем данные пользователя
            user.Email = model.Email;
            user.FullName = model.FullName;
            user.IsActive = model.IsActive;
            user.NormalizedEmail = model.Email.ToUpper();

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                foreach (var error in updateResult.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);
                return View(model);
            }

            // Обновляем роль (удаляем старую, добавляем новую)
            var currentRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, currentRoles);
            if (!string.IsNullOrEmpty(model.SelectedRole))
            {
                await _userManager.AddToRoleAsync(user, model.SelectedRole);
            }

            // Обновляем права
            await _permissionService.SetUserPermissionsAsync(user, model.SelectedPermissions, cancellationToken);

            TempData["Success"] = $"Пользователь {model.Email} успешно обновлен";
            return RedirectToAction(nameof(Users));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при обновлении пользователя {UserId}", model.Id);
            ModelState.AddModelError(string.Empty, "Ошибка при обновлении: " + ex.Message);
            return View(model);
        }
    }

    // GET: /Account/CreateUser
    /// <summary>
    /// Создание нового пользователя с правами (только для администраторов)
    /// </summary>
    [Authorize(Policy = "RequireAdministrator")]
    public async Task<IActionResult> CreateUser(CancellationToken cancellationToken)
    {
        var roles = await _userManagementService.GetAllRolesAsync(cancellationToken);
        var allPermissions = await _permissionService.GetAllPermissionsAsync();

        var model = new UserWithPermissionsViewModel
        {
            AvailableRoles = roles.Select(r => r.Name ?? "").ToList(),
            PermissionsByCategory = allPermissions
                .GroupBy(p => p.Category)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(p => new PermissionViewModel
                    {
                        Name = p.Name,
                        DisplayName = p.DisplayName,
                        Description = p.Description,
                        Category = p.Category,
                        IsAssigned = false
                    }).ToList()
                )
        };
        return View(model);
    }

    // POST: /Account/CreateUser
    /// <summary>
    /// Обработка формы создания пользователя (только для администраторов)
    /// </summary>
    [Authorize(Policy = "RequireAdministrator")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateUser(UserWithPermissionsViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            // Восстанавливаем права для отображения
            var allPermissions = await _permissionService.GetAllPermissionsAsync();
            model.PermissionsByCategory = allPermissions
                .GroupBy(p => p.Category)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(p => new PermissionViewModel
                    {
                        Name = p.Name,
                        DisplayName = p.DisplayName,
                        Description = p.Description,
                        Category = p.Category,
                        IsAssigned = model.SelectedPermissions.Contains(p.Name)
                    }).ToList()
                );
            return View(model);
        }

        var user = new ApplicationUser
        {
            UserName = model.Email,
            Email = model.Email,
            FullName = model.FullName,
            IsActive = model.IsActive,
            EmailConfirmed = true
        };

        var result = await _userManager.CreateAsync(user, model.Password ?? "");
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);

            var allPermissions = await _permissionService.GetAllPermissionsAsync();
            model.PermissionsByCategory = allPermissions
                .GroupBy(p => p.Category)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(p => new PermissionViewModel
                    {
                        Name = p.Name,
                        DisplayName = p.DisplayName,
                        Description = p.Description,
                        Category = p.Category,
                        IsAssigned = model.SelectedPermissions.Contains(p.Name)
                    }).ToList()
                );
            return View(model);
        }

        // Добавляем роль
        if (!string.IsNullOrEmpty(model.SelectedRole))
        {
            await _userManager.AddToRoleAsync(user, model.SelectedRole);
        }

        // Назначаем права
        await _permissionService.SetUserPermissionsAsync(user, model.SelectedPermissions, cancellationToken);

        TempData["Success"] = $"Пользователь {model.Email} успешно создан";
        return RedirectToAction(nameof(Users));
    }

    // POST: /Account/DeleteUser/{id}
    /// <summary>
    /// Удаление пользователя (только для администраторов)
    /// </summary>
    [Authorize(Policy = "RequireAdministrator")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteUser(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _userManagementService.DeleteUserAsync(id, cancellationToken);
            if (result.Succeeded)
                TempData["Success"] = "Пользователь удалён";
            else
                TempData["Error"] = string.Join("; ", result.Errors.Select(e => e.Description));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при удалении пользователя {UserId}", id);
            TempData["Error"] = "Не удалось удалить пользователя";
        }

        return RedirectToAction(nameof(Users));
    }

    // GET: /Account/EditPermissions/{id}
    /// <summary>
    /// Быстрое редактирование прав пользователя (только для администраторов)
    /// </summary>
    [Authorize(Policy = "RequireAdministrator")]
    public async Task<IActionResult> EditPermissions(Guid id, CancellationToken cancellationToken)
    {
        var user = await _userManagementService.GetUserByIdAsync(id, cancellationToken);
        if (user == null) return NotFound();

        var userPermissions = await _permissionService.GetUserPermissionsAsync(user, cancellationToken);
        var allPermissions = await _permissionService.GetAllPermissionsAsync();

        var permissionsByCategory = allPermissions
            .GroupBy(p => p.Category)
            .ToDictionary(
                g => g.Key,
                g => g.Select(p => new PermissionViewModel
                {
                    Name = p.Name,
                    DisplayName = p.DisplayName,
                    Description = p.Description,
                    Category = p.Category,
                    IsAssigned = userPermissions.Contains(p.Name)
                }).ToList()
            );

        var model = new UserWithPermissionsViewModel
        {
            Id = user.Id.ToString(),
            Email = user.Email ?? "",
            FullName = user.FullName ?? "",
            SelectedPermissions = userPermissions,
            PermissionsByCategory = permissionsByCategory
        };

        return PartialView("_EditPermissionsModal", model);
    }

    // POST: /Account/EditPermissions
    /// <summary>
    /// Сохранение изменений прав пользователя (только для администраторов)
    /// </summary>
    [Authorize(Policy = "RequireAdministrator")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditPermissions(UserWithPermissionsViewModel model, CancellationToken cancellationToken)
    {
        try
        {
            var userId = Guid.Parse(model.Id);
            var user = await _userManagementService.GetUserByIdAsync(userId, cancellationToken);
            if (user == null) return NotFound();

            // Обновляем права
            await _permissionService.SetUserPermissionsAsync(user, model.SelectedPermissions, cancellationToken);

            TempData["Success"] = $"Права пользователя {model.Email} обновлены";
            return RedirectToAction(nameof(Users));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при обновлении прав пользователя {UserId}", model.Id);
            ModelState.AddModelError(string.Empty, "Ошибка при обновлении: " + ex.Message);
            return PartialView("_EditPermissionsModal", model);
        }
    }

    #region Вспомогательные методы
    /// <summary>
    /// Метод для перенаправления на локальный URL
    /// </summary>
    private IActionResult RedirectToLocal(string? returnUrl)
    {
        if (Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }
        else
        {
            return RedirectToAction("Index", "Home");
        }
    }
    #endregion
}