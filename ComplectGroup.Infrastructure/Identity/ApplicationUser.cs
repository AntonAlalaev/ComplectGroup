// ComplectGroup.Domain/Entities/ApplicationUser.cs

using Microsoft.AspNetCore.Identity;

namespace ComplectGroup.Infrastructure.Identity
{
    public class ApplicationUser : IdentityUser<Guid>
    {
        public string? FullName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLogin { get; set; }
        public bool IsActive { get; set; } = true;
        
        // Дополнительные свойства при необходимости
    }
}

