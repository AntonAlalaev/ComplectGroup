namespace ComplectGroup.Domain.Entities;

/// <summary>
/// Текущее состояние товара на складе
/// </summary>
public class WarehouseItem
{
    public int Id { get; set; }

    /// <summary>
    /// Деталь на складе
    /// </summary>
    public int PartId { get; set; }
    public required Part Part { get; set; }

    /// <summary>
    /// Текущее количество на складе
    /// </summary>
    public int AvailableQuantity { get; set; } = 0;

    /// <summary>
    /// Зарезервировано для отгрузок
    /// </summary>
    public int ReservedQuantity { get; set; } = 0;

    /// <summary>Дата последнего изменения</summary>
    public DateTime LastModifiedDate { get; set; }
}
