namespace ComplectGroup.Domain.Entities;

/// <summary>
/// Транзакция отгрузки товара со склада
/// Может быть привязана к позиции комплектации или быть без привязки (для корректировок)
/// </summary>
public class ShippingTransaction
{
    public int Id { get; set; }

    /// <summary>
    /// Деталь, которая отгружается
    /// </summary>
    public int PartId { get; set; }
    public required Part Part { get; set; }

    /// <summary>
    /// Позиция комплектации, которая отгружается - привязка конкретной позиции конкретной комплектации
    /// NULL если отгрузка без привязки к позиции (например, для корректировок)
    /// </summary>
    public int? PositionId { get; set; }
    public Position? Position { get; set; }

    /// <summary>
    /// Количество отгруженных деталей
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// Дата отгрузки
    /// </summary>
    public DateTime ShippingDate { get; set; }

    /// <summary>Примечания (номер документа, ответственное лицо и т.д.)</summary>
    public string Notes { get; set; } = string.Empty;

    /// <summary>
    /// ID пользователя, выполнившего отгрузку
    /// </summary>
    public string UserId { get; set; } = string.Empty;
}
