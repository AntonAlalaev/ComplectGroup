using ComplectGroup.Domain.Entities;
using System.ComponentModel.DataAnnotations;

namespace ComplectGroup.Web.Models;

public class ShippingViewModel
{
    [Required(ErrorMessage = "Выберите комплектацию")]
    [Display(Name = "Комплектация")]
    public int ComplectationId { get; set; }

    [Required(ErrorMessage = "Выберите позицию")]
    [Display(Name = "Позиция")]
    public int PositionId { get; set; }

    [Required(ErrorMessage = "Выберите деталь")]
    [Display(Name = "Деталь")]
    public int PartId { get; set; }

    [Required(ErrorMessage = "Введите количество")]
    [Range(1, int.MaxValue, ErrorMessage = "Количество должно быть > 0")]
    [Display(Name = "Количество")]
    public int Quantity { get; set; }

    [Display(Name = "Примечания")]
    [StringLength(500, ErrorMessage = "Максимум 500 символов")]
    public string Notes { get; set; } = string.Empty;

    [Display(Name = "Дата отгрузки")]
    public DateTime ShippingDate { get; set; } = DateTime.Now;

    // Для выпадающих списков
    public List<Complectation> Complectations { get; set; } = new();
    public List<Position> Positions { get; set; } = new();
    public List<Part> Parts { get; set; } = new();
}
