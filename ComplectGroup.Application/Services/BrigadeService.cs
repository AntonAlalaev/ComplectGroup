using ComplectGroup.Application.DTOs;
using ComplectGroup.Application.Interfaces;
using ComplectGroup.Domain.Entities.Planning;
using Microsoft.Extensions.Logging;

namespace ComplectGroup.Application.Services;

/// <summary>
/// Реализация сервиса для работы с бригадами
/// </summary>
public class BrigadeService : IBrigadeService
{
    private readonly IBrigadeRepository _repository;
    private readonly ILogger<BrigadeService> _logger;

    public BrigadeService(IBrigadeRepository repository, ILogger<BrigadeService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<List<BrigadeDto>> GetAllAsync(CancellationToken cancellationToken)
    {
        var brigades = await _repository.GetAllAsync(cancellationToken);
        return brigades.Select(MapToDto).ToList();
    }

    public async Task<BrigadeDto> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        var brigade = await _repository.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Бригада с ID {id} не найдена");

        return MapToDto(brigade);
    }

    public async Task<BrigadeDto> CreateAsync(BrigadeRequest request, CancellationToken cancellationToken)
    {
        Validate(request);

        var name = request.Name.Trim();
        if (await _repository.ExistsByNameAsync(name, null, cancellationToken))
            throw new InvalidOperationException($"Бригада с названием «{name}» уже существует");

        var brigade = new Brigade
        {
            Name = name,
            Description = NormalizeDescription(request.Description),
            IsActive = request.IsActive
        };

        await _repository.AddAsync(brigade, cancellationToken);
        _logger.LogInformation("Создана бригада: {Name}", brigade.Name);

        return MapToDto(brigade);
    }

    public async Task<BrigadeDto> UpdateAsync(int id, BrigadeRequest request, CancellationToken cancellationToken)
    {
        Validate(request);

        var brigade = await _repository.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Бригада с ID {id} не найдена");

        var name = request.Name.Trim();
        if (await _repository.ExistsByNameAsync(name, id, cancellationToken))
            throw new InvalidOperationException($"Бригада с названием «{name}» уже существует");

        brigade.Name = name;
        brigade.Description = NormalizeDescription(request.Description);
        brigade.IsActive = request.IsActive;

        await _repository.UpdateAsync(brigade, cancellationToken);
        _logger.LogInformation("Обновлена бригада с ID {Id}: {Name}", id, brigade.Name);

        return MapToDto(brigade);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken)
    {
        var brigade = await _repository.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Бригада с ID {id} не найдена");

        if (await _repository.HasOperationsAsync(id, cancellationToken))
            throw new InvalidOperationException(
                "Нельзя удалить бригаду: она используется в операциях технологических маршрутов");

        await _repository.DeleteAsync(brigade, cancellationToken);
        _logger.LogInformation("Удалена бригада с ID {Id}", id);
    }

    private static void Validate(BrigadeRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            throw new ArgumentException("Название бригады обязательно");

        if (request.Name.Trim().Length > 200)
            throw new ArgumentException("Название бригады не должно превышать 200 символов");

        if (request.Description?.Trim().Length > 1000)
            throw new ArgumentException("Описание бригады не должно превышать 1000 символов");
    }

    private static string? NormalizeDescription(string? description)
        => string.IsNullOrWhiteSpace(description) ? null : description.Trim();

    private static BrigadeDto MapToDto(Brigade brigade)
    {
        var operations = brigade.Operations ?? new List<PartOperation>();

        return new BrigadeDto
        {
            Id = brigade.Id,
            Name = brigade.Name,
            Description = brigade.Description,
            IsActive = brigade.IsActive,
            OperationsCount = operations.Count,
            TotalPlannedHours = operations.Where(o => o.IsActive).Sum(o => o.PlannedLaborHours)
        };
    }
}
