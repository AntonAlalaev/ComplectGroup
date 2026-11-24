using System.ComponentModel.DataAnnotations;

namespace ComplectGroup.Domain.Entities;

/// <summary>
/// Источник прихода детали на склад
/// </summary>
public enum ReceiptSource
{
    [Display(Name = "Покупка / ввод остатков")]
    Purchase = 0,

    [Display(Name = "Из производственного плана")]
    FromPlan = 1,

    [Display(Name = "Перемещение")]
    Transfer = 2,

    [Display(Name = "Возврат / корректировка")]
    Correction = 3
}
