using ComplectGroup.Domain.Entities;
using System.ComponentModel.DataAnnotations;

namespace ComplectGroup.Web.Models;

public class ReceiptViewModel
{
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

    [Display(Name = "Дата приёмки")]
    public DateTime ReceiptDate { get; set; } = DateTime.Now;

    // Для выпадающего списка
    public List<Part> Parts { get; set; } = new();
}
