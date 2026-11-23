namespace ComplectGroup.Domain.Entities.Planning;

/// <summary>
/// Производственная бригада
/// </summary>
public class Brigade
{
    /// <summary>
    /// Идентификатор бригады
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Название бригады
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Описание бригады
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Признак активности бригады
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Операции, выполняемые бригадой
    /// </summary>
    public ICollection<PartOperation> Operations { get; set; } = new List<PartOperation>();
}
