namespace ComplectGroup.Domain.Entities;

/// <summary>
/// Транзакция приёмки товара на склад
/// Приёмка независима от комплектаций — просто поступление деталей
/// </summary>
public class ReceiptTransaction
{
    public int Id { get; set; }

    /// <summary>
    /// Деталь, которая поступила
    /// </summary>
    public int PartId { get; set; }
    public required Part Part { get; set; }

    /// <summary>
    /// Количество деталей, которые поступили
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// Дата приёмки
    /// </summary>
    public DateTime ReceiptDate { get; set; }

    /// <summary>
    /// Примечания (откуда поступило, номер документа, поставщик и т.д.)
    /// </summary>
    public string Notes { get; set; } = string.Empty;
}
