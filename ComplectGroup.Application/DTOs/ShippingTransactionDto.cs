namespace ComplectGroup.Application.DTOs;

/// <summary>
/// Модель отгрузки детали из комплектации
/// </summary>
public class ShippingTransactionDto
{
    public int Id { get; set; }

    public PartDto Part { get; set; } = default!;

    /// <summary>
    /// Позиция комплектации, для которой отгружаем
    /// </summary>
    public int PositionId { get; set; }

    /// <summary>
    /// Количество в данной отгрузке
    /// вще</summary>
    public int Quantity { get; set; }

    public DateTime ShippingDate { get; set; }

    public string Notes { get; set; } = string.Empty;
}
