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
public class AccountController : Controller
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<AccountController> _logger;
    private readonly IUserManagementService _userManagementService;

    public AccountController(
        IUserManagementService userManagementService,
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        ILogger<AccountController> logger)
    {
        _userManagementService = userManagementService;
        _userManager = userManager;
        _signInManager = signInManager;
        _logger = logger;
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
    /// Метод для отображения списка пользователей
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> Users(CancellationToken cancellationToken)
    {
        var users = await _userManagementService.GetAllUsersAsync(cancellationToken);
        return View(users);
    }

    // GET: /Account/EditUser/{id}
    /// <summary>
    /// Метод для отображения формы редактирования пользователя
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> EditUser(Guid id, CancellationToken cancellationToken)
    {
        var user = await _userManagementService.GetUserByIdAsync(id, cancellationToken);
        if (user == null) return NotFound();
        
        var allRoles = await _userManagementService.GetAllRolesAsync(cancellationToken);
        var userRoles = await _userManager.GetRolesAsync(user);
        
        var model = new EditUserViewModel
        {
            Id = user.Id.ToString(),
            Email = user.Email ?? "",
            FullName = user.FullName ?? "",
            IsActive = user.IsActive,
            AvailableRoles = allRoles.ToDictionary(
                r => r.Name ?? "",
                r => userRoles.Contains(r.Name ?? ""))
        };
        
        return View(model);
    }

    // POST: /Account/EditUser
    /// <summary>
    /// Метод для обработки формы редактирования пользователя
    /// </summary>
    /// <param name="model"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [Authorize(Roles = "Administrator")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditUser(EditUserViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            var allRoles = await _userManagementService.GetAllRolesAsync(cancellationToken);
            model.AvailableRoles = allRoles.ToDictionary(r => r.Name ?? "", r => false);
            return View(model);
        }
        
        try
        {
            var userId = Guid.Parse(model.Id);
            var updateResult = await _userManagementService.UpdateUserAsync(
                userId, model.Email, model.FullName, model.IsActive, cancellationToken);
            
            if (!updateResult.Succeeded)
            {
                foreach (var error in updateResult.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);
                return View(model);
            }
            
            var selectedRoles = model.AvailableRoles
                .Where(kv => kv.Value)
                .Select(kv => kv.Key)
                .ToList();
                
            await _userManagementService.UpdateUserRolesAsync(userId, selectedRoles, cancellationToken);
            

            // Смена пароля (если указан)
            if (!string.IsNullOrWhiteSpace(model.NewPassword))
            {
                var user = await _userManagementService.GetUserByIdAsync(userId, cancellationToken);
                
                // 🔥 ДОБАВИТЬ ПРОВЕРКУ:
                if (user == null)
                {
                    TempData["Error"] = "Пользователь не найден";
                    return RedirectToAction(nameof(Users));
                }
                
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                await _userManager.ResetPasswordAsync(user, token, model.NewPassword);
            }
            
            TempData["Success"] = "Пользователь обновлен";
            return RedirectToAction(nameof(Users));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при обновлении пользователя {UserId}", model.Id);
            ModelState.AddModelError(string.Empty, "Не удалось обновить пользователя");
            return View(model);
        }
    }

    // GET: /Account/CreateUser
    /// <summary>
    /// Метод для отображения формы создания пользователя
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> CreateUser(CancellationToken cancellationToken)
    {
        var roles = await _userManagementService.GetAllRolesAsync(cancellationToken);
        var model = new CreateUserViewModel
        {
            AvailableRoles = roles.Select(r => r.Name ?? "").ToList()  // ← r.Name из ApplicationRole
        };
        return View(model);
    }

    // POST: /Account/CreateUser
    /// <summary>
    /// Метод для обработки формы создания пользователя
    /// </summary>
    /// <param name="model"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [Authorize(Roles = "Administrator")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateUser(CreateUserViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            model.AvailableRoles = await _userManagementService.GetAllRolesAsync(cancellationToken)
                .ContinueWith(t => t.Result.Select(r => r.Name ?? "").ToList());
            return View(model);
        }
        
        var result = await _userManagementService.CreateUserAsync(
            model.Email, model.FullName, model.Password, model.SelectedRoles, cancellationToken);
        
        if (result.Succeeded)
        {
            TempData["Success"] = "Пользователь создан";
            return RedirectToAction(nameof(Users));
        }
        
        foreach (var error in result.Errors)
            ModelState.AddModelError(string.Empty, error.Description);
        
        model.AvailableRoles = await _userManagementService.GetAllRolesAsync(cancellationToken)
            .ContinueWith(t => t.Result.Select(r => r.Name ?? "").ToList());
        return View(model);
    }

    // POST: /Account/DeleteUser/{id}
    /// <summary>
    /// Метод для удаления пользователя
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [Authorize(Roles = "Administrator")]
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

    #region Вспомогательные методы
    /// <summary>
    /// Метод для перенаправления на локальный URL
    /// </summary>
    /// <param name="returnUrl"></param>
    /// <returns></returns>
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