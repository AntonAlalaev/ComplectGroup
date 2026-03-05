// ComplectGroup.Web.Models.Account.CreateUserViewModel.cs
using System.ComponentModel.DataAnnotations;

namespace ComplectGroup.Web.Models.Account;

public class CreateUserViewModel
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;
    
    [Required]
    public string FullName { get; set; } = string.Empty;
    
    [Required, StringLength(100, MinimumLength = 6)]
    public string Password { get; set; } = string.Empty;
    
    [Compare(nameof(Password))]
    public string ConfirmPassword { get; set; } = string.Empty;
        
    /// <summary>
    /// Список выбранных ролей для пользователя
    /// </summary>
    public List<string> SelectedRoles { get; set; } = [];
        
    /// <summary>
    /// Список всех доступных ролей для выбора 
    /// (для чекбоксов)
    /// </summary>
    public List<string> AvailableRoles { get; set; } = [];
}