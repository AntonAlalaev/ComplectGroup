using System.ComponentModel.DataAnnotations;

namespace ComplectGroup.Domain.Entities;

/// <summary>
/// Статус комплектации в процессе отгрузки
/// </summary>
public enum ComplectationStatus
{
    /// <summary>
    /// Черновик / Ещё не начали отгружать
    /// </summary>
    [Display(Name = "Черновик")]
    Draft = 0,

    /// <summary>
    /// Частично отгружена (есть отгрузки, но не всё отгружено)
    /// </summary>
    [Display(Name = "Частично отгружена")]
    PartiallyShipped = 1,

    /// <summary>
    /// Полностью отгружена
    /// </summary>
    [Display(Name = "Отгружена")]
    FullyShipped = 2,

    /// <summary>
    /// Архивирована
    /// </summary>
    [Display(Name = "Архив")]
    Archived = 3
}

/// <summary>
/// Методы расширения для ComplectationStatus
/// </summary>
public static class ComplectationStatusExtensions
{
    public static string GetDescription(this ComplectationStatus status)
    {
        var display = status.GetType()
            .GetField(status.ToString())?
            .GetCustomAttributes(typeof(DisplayAttribute), false)
            .Cast<DisplayAttribute>()
            .FirstOrDefault();
        
        return display?.GetName() ?? status.ToString();
    }
}
