namespace ComplectGroup.Domain.Entities;

public class Part
{
    /// <summary>
    /// Идентификатор детали
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Название детали
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Раздел комплектации в который входит деталь
    /// </summary>
    public required Chapter Chapter { get; set; }
}