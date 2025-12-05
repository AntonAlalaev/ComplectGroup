namespace ComplectGroup.Domain.Entities;

public class Chapter
{
    /// <summary>
    /// Идентификатор раздела комплектации
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Название раздела комплектации
    /// </summary>
    public string Name { get; set; } = string.Empty;
}
