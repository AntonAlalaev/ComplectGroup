using System.ComponentModel.DataAnnotations;

namespace ComplectGroup.Web.Models;

public class BrigadeFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Название бригады обязательно")]
    [StringLength(200, ErrorMessage = "Название бригады не должно превышать 200 символов")]
    public string Name { get; set; } = string.Empty;

    [StringLength(1000, ErrorMessage = "Описание бригады не должно превышать 1000 символов")]
    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;
}
