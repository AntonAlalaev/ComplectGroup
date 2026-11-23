namespace ComplectGroup.Application.DTOs;

/// <summary>
/// Модель типа производственной операции для передачи данных
/// </summary>
public class OperationTypeDto
{
    public int Id { get; set; }

    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Количество операций данного типа
    /// </summary>
    public int OperationsCount { get; set; }

    /// <summary>
    /// Суммарная плановая трудоёмкость операций данного типа
    /// </summary>
    public decimal TotalPlannedHours { get; set; }
}
