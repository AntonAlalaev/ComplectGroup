using ComplectGroup.Application.DTOs;
using ComplectGroup.Application.Interfaces;
using ComplectGroup.Domain.Entities;
using ComplectGroup.Domain.Entities.Planning;
using Microsoft.Extensions.Logging;

namespace ComplectGroup.Application.Services;

/// <summary>
/// Реализация сервиса для работы с операциями технологических маршрутов
/// </summary>
public class PartOperationService : IPartOperationService
{
    private readonly IPartOperationRepository _repository;
    private readonly IPartRepository _partRepository;
    private readonly IBrigadeRepository _brigadeRepository;
    private readonly IOperationTypeRepository _operationTypeRepository;
    private readonly ILogger<PartOperationService> _logger;

    public PartOperationService(
        IPartOperationRepository repository,
        IPartRepository partRepository,
        IBrigadeRepository brigadeRepository,
        IOperationTypeRepository operationTypeRepository,
        ILogger<PartOperationService> logger)
    {
        _repository = repository;
        _partRepository = partRepository;
        _brigadeRepository = brigadeRepository;
        _operationTypeRepository = operationTypeRepository;
        _logger = logger;
    }

    public async Task<List<PartOperationDto>> GetByPartIdAsync(int partId, CancellationToken cancellationToken)
    {
        var operations = await _repository.GetByPartIdAsync(partId, cancellationToken);
        return operations.Select(o => MapToDto(o)).ToList();
    }

    public async Task<PartOperationDto> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        var operation = await _repository.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Операция с ID {id} не найдена");

        return MapToDto(operation);
    }

    public async Task<List<PartPlanningSummaryDto>> GetPartSummariesAsync(
        int? chapterId,
        string? search,
        CancellationToken cancellationToken)
    {
        var parts = await _partRepository.GetAllAsync(cancellationToken);
        var operations = await _repository.GetAllAsync(cancellationToken);

        var operationsByPart = operations
            .GroupBy(o => o.PartId)
            .ToDictionary(g => g.Key, g => g.ToList());

        IEnumerable<Part> filteredParts = parts;

        if (chapterId.HasValue && chapterId.Value > 0)
            filteredParts = filteredParts.Where(p => p.Chapter.Id == chapterId.Value);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            filteredParts = filteredParts.Where(p =>
                p.Name.Contains(term, StringComparison.OrdinalIgnoreCase));
        }

        return filteredParts
            .OrderBy(p => p.Chapter.Name)
            .ThenBy(p => p.Name)
            .Select(part =>
            {
                operationsByPart.TryGetValue(part.Id, out var partOperations);
                partOperations ??= new List<PartOperation>();

                return new PartPlanningSummaryDto
                {
                    PartId = part.Id,
                    PartName = part.Name,
                    ChapterId = part.Chapter.Id,
                    ChapterName = part.Chapter.Name,
                    OperationsCount = partOperations.Count,
                    TotalPlannedHours = partOperations
                        .Where(o => o.IsActive)
                        .Sum(o => o.PlannedLaborHours)
                };
            })
            .ToList();
    }

    public async Task<PartOperationDto> CreateAsync(
        PartOperationRequest request,
        CancellationToken cancellationToken)
    {
        Validate(request);

        var part = await _partRepository.GetByIdAsync(request.PartId, cancellationToken)
            ?? throw new KeyNotFoundException($"Деталь с ID {request.PartId} не найдена");

        var brigade = await _brigadeRepository.GetByIdAsync(request.BrigadeId, cancellationToken)
            ?? throw new KeyNotFoundException($"Бригада с ID {request.BrigadeId} не найдена");

        var operationType = await _operationTypeRepository.GetByIdAsync(request.OperationTypeId, cancellationToken)
            ?? throw new KeyNotFoundException($"Тип операции с ID {request.OperationTypeId} не найден");

        if (await _repository.SequenceExistsAsync(request.PartId, request.SequenceNumber, null, cancellationToken))
            throw new InvalidOperationException(
                $"Для детали уже существует операция с порядковым номером {request.SequenceNumber}");

        var operation = new PartOperation
        {
            PartId = part.Id,
            BrigadeId = brigade.Id,
            OperationTypeId = operationType.Id,
            SequenceNumber = request.SequenceNumber,
            PlannedLaborHours = request.PlannedLaborHours,
            Notes = NormalizeNotes(request.Notes),
            IsActive = request.IsActive
        };

        await _repository.AddAsync(operation, cancellationToken);
        _logger.LogInformation(
            "Добавлена операция {SequenceNumber} для детали {PartId}: {OperationTypeCode}, бригада {BrigadeId}, {Hours} н/ч",
            operation.SequenceNumber, operation.PartId, operationType.Code, operation.BrigadeId, operation.PlannedLaborHours);

        return MapToDto(operation, part, brigade, operationType);
    }

    public async Task<PartOperationDto> UpdateAsync(
        int id,
        PartOperationRequest request,
        CancellationToken cancellationToken)
    {
        Validate(request);

        var existing = await _repository.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Операция с ID {id} не найдена");

        if (existing.PartId != request.PartId)
            throw new InvalidOperationException("Операция принадлежит другой детали");

        var brigade = await _brigadeRepository.GetByIdAsync(request.BrigadeId, cancellationToken)
            ?? throw new KeyNotFoundException($"Бригада с ID {request.BrigadeId} не найдена");

        var operationType = await _operationTypeRepository.GetByIdAsync(request.OperationTypeId, cancellationToken)
            ?? throw new KeyNotFoundException($"Тип операции с ID {request.OperationTypeId} не найден");

        if (await _repository.SequenceExistsAsync(
                existing.PartId, request.SequenceNumber, id, cancellationToken))
        {
            throw new InvalidOperationException(
                $"Для детали уже существует операция с порядковым номером {request.SequenceNumber}");
        }

        var operation = new PartOperation
        {
            Id = id,
            PartId = existing.PartId,
            BrigadeId = brigade.Id,
            OperationTypeId = operationType.Id,
            SequenceNumber = request.SequenceNumber,
            PlannedLaborHours = request.PlannedLaborHours,
            Notes = NormalizeNotes(request.Notes),
            IsActive = request.IsActive
        };

        await _repository.UpdateAsync(operation, cancellationToken);
        _logger.LogInformation(
            "Обновлена операция с ID {Id}: {SequenceNumber}, {Hours} н/ч",
            id, operation.SequenceNumber, operation.PlannedLaborHours);

        var part = await _partRepository.GetByIdAsync(existing.PartId, cancellationToken)
            ?? throw new KeyNotFoundException($"Деталь с ID {existing.PartId} не найдена");

        return MapToDto(operation, part, brigade, operationType);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken)
    {
        var existing = await _repository.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Операция с ID {id} не найдена");

        var operation = new PartOperation { Id = existing.Id };
        await _repository.DeleteAsync(operation, cancellationToken);
        _logger.LogInformation("Удалена операция с ID {Id}", id);
    }

    private static void Validate(PartOperationRequest request)
    {
        if (request.PartId <= 0)
            throw new ArgumentException("Не выбрана деталь");

        if (request.BrigadeId <= 0)
            throw new ArgumentException("Не выбрана бригада");

        if (request.OperationTypeId <= 0)
            throw new ArgumentException("Не выбран тип операции");

        if (request.SequenceNumber <= 0)
            throw new ArgumentException("Порядковый номер должен быть больше 0");

        if (request.PlannedLaborHours <= 0)
            throw new ArgumentException("Плановая трудоёмкость должна быть больше 0");

        if (request.PlannedLaborHours > 100000)
            throw new ArgumentException("Плановая трудоёмкость слишком велика");

        if (request.Notes?.Trim().Length > 2000)
            throw new ArgumentException("Примечание не должно превышать 2000 символов");
    }

    private static string? NormalizeNotes(string? notes)
        => string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();

    private static PartOperationDto MapToDto(
        PartOperation operation,
        Part? part = null,
        Brigade? brigade = null,
        OperationType? operationType = null)
    {
        var partValue = part ?? operation.Part;
        var brigadeValue = brigade ?? operation.Brigade;
        var operationTypeValue = operationType ?? operation.OperationType;

        return new PartOperationDto
        {
            Id = operation.Id,
            PartId = operation.PartId,
            PartName = partValue?.Name ?? string.Empty,
            ChapterName = partValue?.Chapter?.Name ?? string.Empty,
            BrigadeId = operation.BrigadeId,
            BrigadeName = brigadeValue?.Name ?? string.Empty,
            OperationTypeId = operation.OperationTypeId,
            OperationTypeCode = operationTypeValue?.Code ?? string.Empty,
            OperationTypeName = operationTypeValue?.Name ?? string.Empty,
            SequenceNumber = operation.SequenceNumber,
            PlannedLaborHours = operation.PlannedLaborHours,
            Notes = operation.Notes,
            IsActive = operation.IsActive
        };
    }
}
