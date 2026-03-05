// ComplectGroup.Infrastructure/Services/IUserManagementService.cs
using ComplectGroup.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace ComplectGroup.Infrastructure.Services;

/// <summary>
/// Интерфейс сервиса управления пользователями
/// </summary>
public interface IUserManagementService
{
    Task<List<ApplicationUser>> GetAllUsersAsync(CancellationToken ct);
    Task<ApplicationUser?> GetUserByIdAsync(Guid id, CancellationToken ct);
    Task<IdentityResult> UpdateUserAsync(Guid id, string email, string fullName, bool isActive, CancellationToken ct);
    Task<IdentityResult> UpdateUserRolesAsync(Guid userId, List<string> roles, CancellationToken ct);
    Task<IdentityResult> CreateUserAsync(string email, string fullName, string password, List<string> roles, CancellationToken ct);
    Task<IdentityResult> DeleteUserAsync(Guid id, CancellationToken ct);
    Task<List<ApplicationRole>> GetAllRolesAsync(CancellationToken ct);  // ← ApplicationRole вместо IdentityRole

    // ===== Управление правами (claims) =====
    Task<IList<Claim>> GetUserClaimsAsync(ApplicationUser user, CancellationToken ct);
    Task AddClaimAsync(ApplicationUser user, string claimType, string claimValue, CancellationToken ct);
    Task RemoveClaimAsync(ApplicationUser user, string claimType, string claimValue, CancellationToken ct);
    Task SetUserPermissionsAsync(ApplicationUser user, List<string> permissions, CancellationToken ct);
}