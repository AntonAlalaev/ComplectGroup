// ComplectGroup.Web.Models.Account.EditUserViewModel.cs
using System.ComponentModel.DataAnnotations;

namespace ComplectGroup.Web.Models.Account;

public class EditUserViewModel
{
    public string Id { get; set; } = string.Empty;
    
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;
    
    [Required]
    public string FullName { get; set; } = string.Empty;
    
    public bool IsActive { get; set; }
    
    // Роли: ключ = имя роли, значение = назначена ли
    public Dictionary<string, bool> AvailableRoles { get; set; } = [];
    
    // Для смены пароля (опционально)
    [StringLength(100, MinimumLength = 6)]
    public string? NewPassword { get; set; }
    
    [Compare(nameof(NewPassword))]
    public string? ConfirmNewPassword { get; set; }
}