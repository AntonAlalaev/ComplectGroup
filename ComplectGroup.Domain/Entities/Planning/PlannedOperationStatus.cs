using System.ComponentModel.DataAnnotations;

namespace ComplectGroup.Domain.Entities.Planning;

/// <summary>
/// Статус операции в производственном плане
/// </summary>
public enum PlannedOperationStatus
{
    [Display(Name = "Ожидает")]
    Pending = 0,

    [Display(Name = "В работе")]
    InProgress = 1,

    [Display(Name = "Выполнено")]
    Completed = 2,

    [Display(Name = "Пропущено")]
    Skipped = 3
}
