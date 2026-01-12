// ComplectGroup.Web/Extensions/AuthorizationExtensions.cs
using Microsoft.AspNetCore.Authorization;

namespace ComplectGroup.Web.Extensions
{
    public static class AuthorizationExtensions
    {
        public static IServiceCollection AddApplicationAuthorization(this IServiceCollection services)
        {
            services.AddAuthorization(options =>
            {
                // Базовые политики на основе ролей
                options.AddPolicy("RequireAdmin", policy => 
                    policy.RequireRole("Administrator"));
                
                options.AddPolicy("RequireManager", policy => 
                    policy.RequireRole("Administrator", "Manager"));
                
                options.AddPolicy("RequireUser", policy => 
                    policy.RequireRole("Administrator", "Manager", "User"));
                
                // Пример политики на основе утверждений (claims)
                options.AddPolicy("CanEditComplect", policy =>
                    policy.RequireClaim("Permission", "Edit.Complect"));
                
                options.AddPolicy("CanViewReports", policy =>
                    policy.RequireClaim("Permission", "View.Reports"));
            });

            return services;
        }
    }
}
