namespace ComplectGroup.Web.Models;

/// <summary>
/// Модель для отображения пагинации
/// </summary>
public class PaginationInfo
{
    /// <summary>
    /// Текущая страница
    /// </summary>
    public int CurrentPage { get; set; } = 1;

    /// <summary>
    /// Всего страниц
    /// </summary>
    public int TotalPages { get; set; }

    /// <summary>
    /// Всего записей
    /// </summary>
    public int TotalItems { get; set; }

    /// <summary>
    /// Записей на странице
    /// </summary>
    public int PageSize { get; set; } = 20;

    /// <summary>
    /// Есть ли предыдущая страница
    /// </summary>
    public bool HasPrevious => CurrentPage > 1;

    /// <summary>
    /// Есть ли следующая страница
    /// </summary>
    public bool HasNext => CurrentPage < TotalPages;
}
