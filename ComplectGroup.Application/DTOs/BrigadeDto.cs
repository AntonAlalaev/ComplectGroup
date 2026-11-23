namespace ComplectGroup.Application.DTOs;

/// <summary>
/// Модель бригады для передачи данных
/// </summary>
public class BrigadeDto
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public bool IsActive { get; set; }

    /// <summary>
    /// Количество операций, выполняемых бригадой
    /// </summary>
    public int OperationsCount { get; set; }

    /// <summary>
    /// Суммарная плановая трудоёмкость операций бригады
    /// </summary>
    public decimal TotalPlannedHours { get; set; }
}
