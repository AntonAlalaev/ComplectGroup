namespace ComplectGroup.Domain.Entities.Planning;

/// <summary>
/// Тип производственной операции (резка, гибка, сварка, покраска, упаковка и т.д.)
/// </summary>
public class OperationType
{
    /// <summary>
    /// Идентификатор типа операции
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Уникальный код типа операции
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Название типа операции
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Операции данного типа
    /// </summary>
    public ICollection<PartOperation> Operations { get; set; } = new List<PartOperation>();
}
