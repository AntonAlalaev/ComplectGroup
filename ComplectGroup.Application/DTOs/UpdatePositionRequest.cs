namespace ComplectGroup.Application.DTOs;

/// <summary>
/// Запрос на обновление позиции комплектации
/// </summary>
public class UpdatePositionRequest
{
    /// <summary>
    /// Идентификатор позиции (если null — создаётся новая)
    /// </summary>
    public int? Id { get; set; }

    /// <summary>
    /// Идентификатор детали
    /// </summary>
    public int? PartId { get; set; }

    /// <summary>
    /// Количество
    /// </summary>
    public int? Quantity { get; set; }

    /// <summary>
    /// Признак удаления (опционально)
    /// </summary>
    public bool? IsDeleted { get; set; }
}
