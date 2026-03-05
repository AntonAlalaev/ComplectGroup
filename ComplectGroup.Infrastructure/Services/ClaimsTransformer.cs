using ComplectGroup.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;

namespace ComplectGroup.Infrastructure.Services;

/// <summary>
/// Преобразователь claims - добавляет права пользователя при каждой аутентификации
/// </summary>
public class ClaimsTransformer : IClaimsTransformation
{
    private readonly UserManager<ApplicationUser> _userManager;

    public ClaimsTransformer(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        if (principal.Identity is not ClaimsIdentity identity || !principal.Identity.IsAuthenticated)
        {
            return principal;
        }

        var userId = _userManager.GetUserId(principal);
        if (string.IsNullOrEmpty(userId))
        {
            return principal;
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return principal;
        }

        // Получаем claims пользователя из базы
        var userClaims = await _userManager.GetClaimsAsync(user);

        // Добавляем claims из базы в текущий principal
        foreach (var claim in userClaims.Where(c => !identity.HasClaim(c.Type, c.Value)))
        {
            identity.AddClaim(claim);
        }

        // Добавляем роли как claims
        var roles = await _userManager.GetRolesAsync(user);
        foreach (var role in roles)
        {
            var roleClaim = new Claim(ClaimTypes.Role, role);
            if (!identity.HasClaim(roleClaim.Type, roleClaim.Value))
            {
                identity.AddClaim(roleClaim);
            }
        }

        return principal;
    }
}
