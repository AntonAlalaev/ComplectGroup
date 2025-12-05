using ComplectGroup.Application.DTOs;
using ComplectGroup.Application.Interfaces;
using ComplectGroup.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace ComplectGroup.Application.Services;

/// <summary>
/// Реализация сервиса для работы с главами
/// </summary>
public class ChapterService : IChapterService
{
    private readonly IChapterRepository _repository;
    private readonly ILogger<ChapterService> _logger;

    public ChapterService(IChapterRepository repository, ILogger<ChapterService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<ChapterDto?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        var chapter = await _repository.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Раздел комплектации с ID {id} не найдена");

        return MapToDto(chapter);
    }

    public async Task<List<ChapterDto>> GetAllAsync(CancellationToken cancellationToken)
    {
        var chapters = await _repository.GetAllAsync(cancellationToken);
        return chapters.Select(MapToDto).ToList();
    }

    public async Task<ChapterDto> CreateAsync(string name, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Название Раздела комплектации обязательно");

        var chapter = new Chapter { Name = name };
        await _repository.AddAsync(chapter, cancellationToken);

        _logger.LogInformation("Создан Раздел комплектации: {Name}", name);
        return MapToDto(chapter);
    }

    public async Task<ChapterDto> UpdateAsync(int id, string name, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Название Раздела комплектации обязательно");

        var chapter = await _repository.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Раздел комплектации с ID {id} не найден");

        chapter.Name = name;
        await _repository.UpdateAsync(chapter, cancellationToken);

        _logger.LogInformation("Обновлен раздел комплектации с ID {Id}: {Name}", id, name);
        return MapToDto(chapter);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken)
    {
        var chapter = await _repository.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Раздел комплектации с ID {id} не найден");

        await _repository.DeleteAsync(chapter, cancellationToken);
        _logger.LogInformation("Удален Раздел комплектации с ID {Id}", id);
    }

    private ChapterDto MapToDto(Chapter chapter) =>
        new ChapterDto { Id = chapter.Id, Name = chapter.Name };
}
