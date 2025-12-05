namespace ComplectGroup.Application.DTOs;

/// <summary>
/// Модель детали для передачи данных в API
/// </summary>
public class PartDto
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
    /// Раздел комплектации, к которому относится деталь
    /// </summary>
    public ChapterDto Chapter { get; set; } = default!;
}