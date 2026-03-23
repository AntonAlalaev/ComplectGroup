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
    /// NULL если отгрузка без привязки к позиции (например, для корректировок)
    /// </summary>
    public int? PositionId { get; set; }

    /// <summary>
    /// Количество в данной отгрузке
    /// </summary>
    public int Quantity { get; set; }

    public DateTime ShippingDate { get; set; }

    public string Notes { get; set; } = string.Empty;

    /// <summary>
    /// ID пользователя, выполнившего отгрузку
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Имя пользователя, выполнившего отгрузку
    /// </summary>
    public string UserName { get; set; } = string.Empty;
}
