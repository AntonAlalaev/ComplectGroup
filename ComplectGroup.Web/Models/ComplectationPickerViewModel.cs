using ComplectGroup.Application.DTOs;
using ComplectGroup.Domain.Entities;

namespace ComplectGroup.Web.Models;

/// <summary>
/// ViewModel для фильтра выбора комплектации в модальном окне
/// </summary>
public class ComplectationPickerViewModel
{
    // ===== ПАРАМЕТРЫ ФИЛЬТРА =====
    
    /// <summary>
    /// Поиск по номеру комплектации
    /// </summary>
    public string? SearchNumber { get; set; }

    /// <summary>
    /// Поиск по заказчику
    /// </summary>
    public string? SearchCustomer { get; set; }

    /// <summary>
    /// Поиск по менеджеру
    /// </summary>
    public string? SearchManager { get; set; }

    /// <summary>
    /// Статус комплектации
    /// </summary>
    public ComplectationStatus? Status { get; set; }

    /// <summary>
    /// Дата отгрузки с
    /// </summary>
    public DateOnly? DateFrom { get; set; }

    /// <summary>
    /// Дата отгрузки по
    /// </summary>
    public DateOnly? DateTo { get; set; }

    /// <summary>
    /// Быстрый пресет
    /// </summary>
    public string? Preset { get; set; }

    // ===== ПАГИНАЦИЯ =====
    
    /// <summary>
    /// Текущая страница
    /// </summary>
    public int PageNumber { get; set; } = 1;

    /// <summary>
    /// Размер страницы
    /// </summary>
    public int PageSize { get; set; } = 20;

    // ===== РЕЗУЛЬТАТЫ =====
    
    /// <summary>
    /// Список комплектаций для отображения
    /// </summary>
    public List<ComplectationDto> Items { get; set; } = new();

    /// <summary>
    /// Общее количество
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// Общее количество страниц
    /// </summary>
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);

    /// <summary>
    /// Есть ли предыдущая страница
    /// </summary>
    public bool HasPrevious => PageNumber > 1;

    /// <summary>
    /// Есть ли следующая страница
    /// </summary>
    public bool HasNext => PageNumber < TotalPages;

    /// <summary>
    /// Выбранный ID комплектации (для подсветки в списке)
    /// </summary>
    public int? SelectedId { get; set; }

    /// <summary>
    /// Доступные размеры страницы
    /// </summary>
    public static List<int> AvailablePageSizes => new() { 10, 20, 50, 100 };

    /// <summary>
    /// Применить быстрый пресет
    /// </summary>
    public void ApplyPreset()
    {
        if (string.IsNullOrEmpty(Preset)) return;

        switch (Preset.ToLower())
        {
            case "today":
                DateFrom = DateOnly.FromDateTime(DateTime.Today);
                DateTo = DateFrom;
                break;

            case "week":
                var startOfWeek = DateTime.Today.StartOfWeek(DayOfWeek.Monday);
                DateFrom = DateOnly.FromDateTime(startOfWeek);
                DateTo = DateOnly.FromDateTime(DateTime.Today);
                break;

            case "month":
                DateFrom = new DateOnly(DateTime.Today.Year, DateTime.Today.Month, 1);
                DateTo = DateOnly.FromDateTime(DateTime.Today);
                break;

            case "draft":
                Status = ComplectationStatus.Draft;
                break;

            case "shipped":
                Status = ComplectationStatus.FullyShipped;
                break;

            case "partial":
                Status = ComplectationStatus.PartiallyShipped;
                break;

            case "all":
            default:
                DateFrom = null;
                DateTo = null;
                Status = null;
                break;
        }
    }
}

/// <summary>
/// Расширения для DateTime
/// </summary>
public static class DateTimeExtensions
{
    /// <summary>
    /// Получить начало недели
    /// </summary>
    public static DateTime StartOfWeek(this DateTime dt, DayOfWeek startOfWeek)
    {
        int diff = (7 + (dt.DayOfWeek - startOfWeek)) % 7;
        return dt.AddDays(-1 * diff).Date;
    }
}
