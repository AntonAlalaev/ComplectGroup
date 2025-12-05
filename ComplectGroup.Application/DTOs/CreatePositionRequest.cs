namespace ComplectGroup.Application.DTOs;

/// <summary>
/// Запрос на создание позиции комплектации
/// </summary>
public class CreatePositionRequest
{
    /// <summary>
    /// Идентификатор детали (обязательно)
    /// </summary>
    public int PartId { get; set; }

    /// <summary>
    /// Количество (должно быть > 0)
    /// </summary>
    public int Quantity { get; set; }
}
