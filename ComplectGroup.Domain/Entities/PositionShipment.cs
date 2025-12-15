namespace ComplectGroup.Domain.Entities;

/// <summary>
/// Отслеживание отгруженного количества по позиции
/// Создаётся первый раз при первой отгрузке, обновляется при каждой следующей отгрузке
/// </summary>
public class PositionShipment
{
    public int Id { get; set; }

    /// <summary>Позиция комплектации</summary>
    public int PositionId { get; set; }
    public required Position Position { get; set; }

    /// <summary>Всего отгружено за все транзакции</summary>
    public int ShippedQuantity { get; set; }

    /// <summary>Дата первой отгрузки</summary>
    public DateTime? FirstShippedDate { get; set; }

    /// <summary>Дата последней отгрузки</summary>
    public DateTime? LastShippedDate { get; set; }
}
