using ComplectGroup.Application.DTOs;
using ComplectGroup.Domain.Entities;

namespace ComplectGroup.Application.Models;

/// <summary>
/// Модель фильтрации комплектаций
/// </summary>
public class ComplectationFilterViewModel
{
    // ===== ПОИСК =====
    
    /// <summary>
    /// Поиск по номеру комплектации (частичное совпадение)
    /// </summary>
    public string? SearchNumber { get; set; }

    /// <summary>
    /// Поиск по заказчику (частичное совпадение)
    /// </summary>
    public string? SearchCustomer { get; set; }

    /// <summary>
    /// Поиск по менеджеру (частичное совпадение)
    /// </summary>
    public string? SearchManager { get; set; }

    /// <summary>
    /// Поиск по адресу (частичное совпадение)
    /// </summary>
    public string? SearchAddress { get; set; }

    // ===== ФИЛЬТРЫ =====

    /// <summary>
    /// Дата отгрузки с (включительно)
    /// </summary>
    public DateOnly? DateFrom { get; set; }

    /// <summary>
    /// Дата отгрузки по (включительно)
    /// </summary>
    public DateOnly? DateTo { get; set; }

    /// <summary>
    /// Статус комплектации
    /// </summary>
    public ComplectationStatus? Status { get; set; }

    /// <summary>
    /// Только игнорируемые
    /// </summary>
    public bool? IsIgnored { get; set; }

    /// <summary>
    /// Только полностью отгруженные
    /// </summary>
    public bool? IsFullyShipped { get; set; }

    // ===== СОРТИРОВКА =====

    /// <summary>
    /// Поле для сортировки
    /// </summary>
    public string SortBy { get; set; } = "ShippingDate";

    /// <summary>
    /// Направление сортировки (true = по убыванию)
    /// </summary>
    public bool SortDescending { get; set; } = true;

    // ===== ПАГИНАЦИЯ =====

    /// <summary>
    /// Текущая страница (начинается с 1)
    /// </summary>
    public int PageNumber { get; set; } = 1;

    /// <summary>
    /// Размер страницы
    /// </summary>
    public int PageSize { get; set; } = 20;

    /// <summary>
    /// Доступные размеры страницы
    /// </summary>
    public static List<int> AvailablePageSizes => new() { 10, 20, 50, 100, 200 };

    // ===== ПРЕСЕТЫ =====

    /// <summary>
    /// Быстрый пресет фильтра
    /// </summary>
    public string? Preset { get; set; } // "all", "today", "week", "month", "draft", "shipped"

    /// <summary>
    /// Применить пресет
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

            case "partially":
                Status = ComplectationStatus.PartiallyShipped;
                break;

            case "shipped":
                IsFullyShipped = true;
                break;

            case "ignored":
                IsIgnored = true;
                break;

            case "all":
            default:
                // Сброс фильтров
                DateFrom = null;
                DateTo = null;
                Status = null;
                IsIgnored = null;
                IsFullyShipped = null;
                break;
        }
    }

    /// <summary>
    /// Сбросить все фильтры
    /// </summary>
    public void Reset()
    {
        SearchNumber = null;
        SearchCustomer = null;
        SearchManager = null;
        SearchAddress = null;
        DateFrom = null;
        DateTo = null;
        Status = null;
        IsIgnored = null;
        IsFullyShipped = null;
        Preset = null;
        PageNumber = 1;
    }

    /// <summary>
    /// Получить словарь параметров для URL
    /// </summary>
    public Dictionary<string, string> ToRouteValues()
    {
        var route = new Dictionary<string, string>();

        if (!string.IsNullOrEmpty(SearchNumber)) route["SearchNumber"] = SearchNumber;
        if (!string.IsNullOrEmpty(SearchCustomer)) route["SearchCustomer"] = SearchCustomer;
        if (!string.IsNullOrEmpty(SearchManager)) route["SearchManager"] = SearchManager;
        if (!string.IsNullOrEmpty(SearchAddress)) route["SearchAddress"] = SearchAddress;
        if (DateFrom.HasValue) route["DateFrom"] = DateFrom.Value.ToString("yyyy-MM-dd");
        if (DateTo.HasValue) route["DateTo"] = DateTo.Value.ToString("yyyy-MM-dd");
        if (Status.HasValue) route["Status"] = ((int)Status.Value).ToString();
        if (IsIgnored.HasValue) route["IsIgnored"] = IsIgnored.Value.ToString().ToLower();
        if (IsFullyShipped.HasValue) route["IsFullyShipped"] = IsFullyShipped.Value.ToString().ToLower();
        if (!string.IsNullOrEmpty(Preset)) route["Preset"] = Preset;
        
        route["SortBy"] = SortBy;
        route["SortDescending"] = SortDescending.ToString().ToLower();
        route["PageNumber"] = PageNumber.ToString();
        route["PageSize"] = PageSize.ToString();

        return route;
    }
}

/// <summary>
/// Результат фильтрации с пагинацией
/// </summary>
public class PagedComplectationsResult
{
    /// <summary>
    /// Список комплектаций на текущей странице
    /// </summary>
    public List<ComplectationDto> Items { get; set; } = new();

    /// <summary>
    /// Общее количество элементов
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// Текущая страница
    /// </summary>
    public int PageNumber { get; set; }

    /// <summary>
    /// Размер страницы
    /// </summary>
    public int PageSize { get; set; }

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
