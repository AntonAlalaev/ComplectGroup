using ComplectGroup.Domain.Entities;

namespace ComplectGroup.Domain.Entities.Planning;

/// <summary>
/// Плановая операция в производственном плане
/// </summary>
public class PlannedOperation
{
    public int Id { get; set; }

    public int ProductionPlanItemId { get; set; }

    public ProductionPlanItem ProductionPlanItem { get; set; } = null!;

    /// <summary>
    /// Ссылка на операцию технологического маршрута (трудовую норму)
    /// </summary>
    public int PartOperationId { get; set; }

    public PartOperation PartOperation { get; set; } = null!;

    public int BrigadeId { get; set; }

    public Brigade Brigade { get; set; } = null!;

    /// <summary>
    /// Плановая трудоёмкость (Quantity × норма времени)
    /// </summary>
    public decimal PlannedHours { get; set; }

    /// <summary>
    /// Статус операции
    /// </summary>
    public PlannedOperationStatus Status { get; set; } = PlannedOperationStatus.Pending;

    /// <summary>
    /// Фактические трудозатраты (заполняются при отметке выполнения)
    /// </summary>
    public decimal? ActualHours { get; set; }

    /// <summary>
    /// Фактически изготовленное количество деталей, шт. (заполняется при отметке выполнения)
    /// </summary>
    public int? ActualQuantity { get; set; }

    /// <summary>
    /// Дата фактического завершения
    /// </summary>
    public DateTime? CompletedAt { get; set; }

    /// <summary>
    /// Причина пропуска (при статусе Skipped)
    /// </summary>
    public string? SkipReason { get; set; }

    /// <summary>
    /// Отметить операцию выполненной
    /// </summary>
    public void Complete(decimal actualHours, int? actualQuantity = null)
    {
        if (actualHours <= 0)
            throw new ArgumentException("Фактические трудозатраты должны быть больше 0", nameof(actualHours));

        if (actualQuantity is < 0)
            throw new ArgumentException("Фактическое количество не может быть отрицательным", nameof(actualQuantity));

        Status = PlannedOperationStatus.Completed;
        ActualHours = actualHours;
        ActualQuantity = actualQuantity;
        CompletedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Перевести операцию в работу
    /// </summary>
    public void Start()
    {
        if (Status != PlannedOperationStatus.Pending)
            throw new InvalidOperationException("В работу можно перевести только ожидающую операцию");

        Status = PlannedOperationStatus.InProgress;
    }

    /// <summary>
    /// Пропустить операцию с указанием причины
    /// </summary>
    public void Skip(string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Необходимо указать причину пропуска", nameof(reason));

        Status = PlannedOperationStatus.Skipped;
        SkipReason = reason.Trim();
    }
}
