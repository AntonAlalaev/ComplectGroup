using ComplectGroup.Application.DTOs;
using ComplectGroup.Application.Interfaces;
using ComplectGroup.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace ComplectGroup.Application.Services;

/// <summary>
/// Реализация сервиса для работы с деталями
/// </summary>
public class PartService : IPartService
{
    private readonly IPartRepository _repository;
    private readonly IChapterRepository _chapterRepository;
    private readonly ILogger<PartService> _logger;

    public PartService(
        IPartRepository repository,
        IChapterRepository chapterRepository,
        ILogger<PartService> logger)
    {
        _repository = repository;
        _chapterRepository = chapterRepository;
        _logger = logger;
    }

    public async Task<PartDto?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        var part = await _repository.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Деталь с ID {id} не найдена");

        return MapToDto(part);
    }

    public async Task<List<PartDto>> GetAllAsync(CancellationToken cancellationToken)
    {
        var parts = await _repository.GetAllAsync(cancellationToken);
        return parts.Select(MapToDto).ToList();
    }

    public async Task<List<PartDto>> GetByChapterIdAsync(int chapterId, CancellationToken cancellationToken)
    {
        var parts = await _repository.GetByChapterIdAsync(chapterId, cancellationToken);
        return parts.Select(MapToDto).ToList();
    }

    public async Task<PartDto> CreateAsync(string name, int chapterId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Название детали обязательно");

        var chapter = await _chapterRepository.GetByIdAsync(chapterId, cancellationToken)
            ?? throw new KeyNotFoundException($"Глава с ID {chapterId} не найдена");

        var part = new Part { Name = name, Chapter = chapter };
        await _repository.AddAsync(part, cancellationToken);

        _logger.LogInformation("Создана деталь: {Name} в главе {ChapterId}", name, chapterId);
        return MapToDto(part);
    }

    public async Task<PartDto> UpdateAsync(int id, string name, int chapterId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Название детали обязательно");

        var part = await _repository.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Деталь с ID {id} не найдена");

        var chapter = await _chapterRepository.GetByIdAsync(chapterId, cancellationToken)
            ?? throw new KeyNotFoundException($"Глава с ID {chapterId} не найдена");

        part.Name = name;
        part.Chapter = chapter;
        await _repository.UpdateAsync(part, cancellationToken);

        _logger.LogInformation("Обновлена деталь с ID {Id}: {Name}", id, name);
        return MapToDto(part);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken)
    {
        var part = await _repository.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Деталь с ID {id} не найдена");

        await _repository.DeleteAsync(part, cancellationToken);
        _logger.LogInformation("Удалена деталь с ID {Id}", id);
    }

    private PartDto MapToDto(Part part) =>
        new PartDto
        {
            Id = part.Id,
            Name = part.Name,
            Chapter = new ChapterDto { Id = part.Chapter.Id, Name = part.Chapter.Name }
        };
}
