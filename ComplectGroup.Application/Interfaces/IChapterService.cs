using ComplectGroup.Application.DTOs;

namespace ComplectGroup.Application.Interfaces;

/// <summary>
/// Сервис для работы с главами (разделами комплектации)
/// </summary>
public interface IChapterService
{
    /// <summary>
    /// Получить главу по ID
    /// </summary>
    Task<ChapterDto?> GetByIdAsync(int id, CancellationToken cancellationToken);

    /// <summary>
    /// Получить все главы
    /// </summary>
    Task<List<ChapterDto>> GetAllAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Создать новую главу
    /// </summary>
    Task<ChapterDto> CreateAsync(string name, CancellationToken cancellationToken);

    /// <summary>
    /// Обновить главу
    /// </summary>
    Task<ChapterDto> UpdateAsync(int id, string name, CancellationToken cancellationToken);

    /// <summary>
    /// Удалить главу
    /// </summary>
    Task DeleteAsync(int id, CancellationToken cancellationToken);
}
