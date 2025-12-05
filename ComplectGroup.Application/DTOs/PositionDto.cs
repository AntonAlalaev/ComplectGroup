namespace ComplectGroup.Application.DTOs;

/// <summary>
/// Модель позиции в комплектации
/// </summary>
public class PositionDto
{
    /// <summary>
    /// Идентификатор позиции в комплектации
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Деталь, указанная в позиции
    /// </summary>
    public PartDto Part { get; set; } = default!;

    /// <summary>
    /// Количество единиц данной детали
    /// </summary>
    public int Quantity { get; set; }
}