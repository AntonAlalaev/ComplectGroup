namespace ComplectGroup.Application.DTOs;

/// <summary>
/// Модель раздела комплектации для передачи данных в API
/// </summary>
public class ChapterDto
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