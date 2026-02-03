// ComplectGroup.Web/Models/CorrectionViewModel.cs
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ComplectGroup.Web.Models;

public class CorrectionViewModel
{
    [Required(ErrorMessage = "Выберите старую деталь")]
    [Display(Name = "Старая деталь (списание)")]
    public int OldPartId { get; set; }
    
    [Required(ErrorMessage = "Выберите новую деталь")]
    [Display(Name = "Новая деталь (приход)")]
    public int NewPartId { get; set; }
    
    [Required(ErrorMessage = "Укажите количество")]
    [Range(1, int.MaxValue, ErrorMessage = "Количество должно быть больше 0")]
    [Display(Name = "Количество")]
    public int Quantity { get; set; }
    
    [Display(Name = "Примечания")]
    public string Notes { get; set; } = string.Empty;
    
    public SelectList Parts { get; set; } = new SelectList(new List<SelectListItem>());
}