using System.ComponentModel.DataAnnotations;
using ComplectGroup.Web.ModelBinders;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ComplectGroup.Web.Models;

public class PartOperationFormViewModel
{
    public int Id { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Выберите деталь")]
    public int PartId { get; set; }

    public string PartName { get; set; } = string.Empty;

    public string ChapterName { get; set; } = string.Empty;

    [Range(1, int.MaxValue, ErrorMessage = "Порядковый номер должен быть больше 0")]
    public int SequenceNumber { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Выберите тип операции")]
    public int OperationTypeId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Выберите бригаду")]
    public int BrigadeId { get; set; }

    [ModelBinder(typeof(DecimalCommaModelBinder))]
    public decimal PlannedLaborHours { get; set; } = 1;

    [StringLength(2000, ErrorMessage = "Примечание не должно превышать 2000 символов")]
    public string? Notes { get; set; }

    public bool IsActive { get; set; } = true;

    public List<SelectListItem> OperationTypes { get; set; } = new();

    public List<SelectListItem> Brigades { get; set; } = new();
}
