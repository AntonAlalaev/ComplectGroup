using System.ComponentModel.DataAnnotations;

namespace ComplectGroup.Domain.Entities;

/// <summary>
/// Статус производственного плана
/// </summary>
public enum ProductionPlanStatus
{
    [Display(Name = "Черновик")]
    Draft = 0,

    [Display(Name = "Утверждён")]
    Approved = 1,

    [Display(Name = "В работе")]
    InProgress = 2,

    [Display(Name = "Закрыт")]
    Closed = 3
}
