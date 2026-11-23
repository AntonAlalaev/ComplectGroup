namespace ComplectGroup.Domain.Entities.Planning;

/// <summary>
/// Строка технологического маршрута детали
/// </summary>
public class PartOperation
{
    /// <summary>
    /// Идентификатор операции
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Идентификатор детали
    /// </summary>
    public int PartId { get; set; }

    /// <summary>
    /// Деталь, к которой относится операция
    /// </summary>
    public Part Part { get; set; } = null!;

    /// <summary>
    /// Идентификатор бригады, выполняющей операцию
    /// </summary>
    public int BrigadeId { get; set; }

    /// <summary>
    /// Бригада, выполняющая операцию
    /// </summary>
    public Brigade Brigade { get; set; } = null!;

    /// <summary>
    /// Идентификатор типа операции
    /// </summary>
    public int OperationTypeId { get; set; }

    /// <summary>
    /// Тип операции
    /// </summary>
    public OperationType OperationType { get; set; } = null!;

    /// <summary>
    /// Порядок операции в технологическом маршруте
    /// </summary>
    public int SequenceNumber { get; set; }

    /// <summary>
    /// Плановая трудоёмкость операции в нормо-часах
    /// </summary>
    public decimal PlannedLaborHours { get; set; }

    /// <summary>
    /// Примечания к операции
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Признак активности операции
    /// </summary>
    public bool IsActive { get; set; } = true;
}
