namespace ComplectGroup.Application.DTOs;

/// <summary>
/// Сводка по технологическому маршруту детали
/// </summary>
public class PartPlanningSummaryDto
{
    public int PartId { get; set; }

    public string PartName { get; set; } = string.Empty;

    public int ChapterId { get; set; }

    public string ChapterName { get; set; } = string.Empty;

    /// <summary>
    /// Количество операций в маршруте
    /// </summary>
    public int OperationsCount { get; set; }

    /// <summary>
    /// Суммарная плановая трудоёмкость активных операций
    /// </summary>
    public decimal TotalPlannedHours { get; set; }
}
