namespace ComplectGroup.Application.DTOs;

public class PositionShipmentDto
{
    public int Id { get; set; }
    public int PositionId { get; set; }

    /// <summary>Уже отгружено</summary>
    public int ShippedQuantity { get; set; }

    /// <summary>Требуется отгрузить (из Position.RequiredQuantity)</summary>
    public int RequiredQuantity { get; set; }

    /// <summary>Осталось отгрузить</summary>
    public int RemainingQuantity => RequiredQuantity - ShippedQuantity;

    public DateTime? FirstShippedDate { get; set; }
    public DateTime? LastShippedDate { get; set; }

    /// <summary>Статус: "NotShipped", "PartiallyShipped", "FullyShipped"</summary>
    public string Status =>
        ShippedQuantity == 0 ? "NotShipped" :
        ShippedQuantity < RequiredQuantity ? "PartiallyShipped" :
        "FullyShipped";
}
