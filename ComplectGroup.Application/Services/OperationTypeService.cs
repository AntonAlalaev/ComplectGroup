using ComplectGroup.Application.DTOs;
using ComplectGroup.Application.Interfaces;
using ComplectGroup.Domain.Entities.Planning;
using Microsoft.Extensions.Logging;

namespace ComplectGroup.Application.Services;

/// <summary>
/// Реализация сервиса для работы с типами операций
/// </summary>
public class OperationTypeService : IOperationTypeService
{
    private readonly IOperationTypeRepository _repository;
    private readonly ILogger<OperationTypeService> _logger;

    public OperationTypeService(IOperationTypeRepository repository, ILogger<OperationTypeService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<List<OperationTypeDto>> GetAllAsync(CancellationToken cancellationToken)
    {
        var operationTypes = await _repository.GetAllAsync(cancellationToken);
        return operationTypes.Select(MapToDto).ToList();
    }

    public async Task<OperationTypeDto> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        var operationType = await _repository.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Тип операции с ID {id} не найден");

        return MapToDto(operationType);
    }

    public async Task<OperationTypeDto> CreateAsync(OperationTypeRequest request, CancellationToken cancellationToken)
    {
        Validate(request);

        var code = request.Code.Trim();
        if (await _repository.ExistsByCodeAsync(code, null, cancellationToken))
            throw new InvalidOperationException($"Тип операции с кодом «{code}» уже существует");

        var operationType = new OperationType
        {
            Code = code,
            Name = request.Name.Trim()
        };

        await _repository.AddAsync(operationType, cancellationToken);
        _logger.LogInformation("Создан тип операции: {Code} — {Name}", operationType.Code, operationType.Name);

        return MapToDto(operationType);
    }

    public async Task<OperationTypeDto> UpdateAsync(int id, OperationTypeRequest request, CancellationToken cancellationToken)
    {
        Validate(request);

        var operationType = await _repository.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Тип операции с ID {id} не найден");

        var code = request.Code.Trim();
        if (await _repository.ExistsByCodeAsync(code, id, cancellationToken))
            throw new InvalidOperationException($"Тип операции с кодом «{code}» уже существует");

        operationType.Code = code;
        operationType.Name = request.Name.Trim();

        await _repository.UpdateAsync(operationType, cancellationToken);
        _logger.LogInformation("Обновлён тип операции с ID {Id}: {Code} — {Name}", id, operationType.Code, operationType.Name);

        return MapToDto(operationType);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken)
    {
        var operationType = await _repository.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Тип операции с ID {id} не найден");

        if (await _repository.HasOperationsAsync(id, cancellationToken))
            throw new InvalidOperationException(
                "Нельзя удалить тип операции: он используется в технологических маршрутах деталей");

        await _repository.DeleteAsync(operationType, cancellationToken);
        _logger.LogInformation("Удалён тип операции с ID {Id}", id);
    }

    private static void Validate(OperationTypeRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Code))
            throw new ArgumentException("Код операции обязателен");

        if (request.Code.Trim().Length > 50)
            throw new ArgumentException("Код операции не должен превышать 50 символов");

        if (string.IsNullOrWhiteSpace(request.Name))
            throw new ArgumentException("Название операции обязательно");

        if (request.Name.Trim().Length > 200)
            throw new ArgumentException("Название операции не должно превышать 200 символов");
    }

    private static OperationTypeDto MapToDto(OperationType operationType)
    {
        var operations = operationType.Operations ?? new List<PartOperation>();

        return new OperationTypeDto
        {
            Id = operationType.Id,
            Code = operationType.Code,
            Name = operationType.Name,
            OperationsCount = operations.Count,
            TotalPlannedHours = operations.Where(o => o.IsActive).Sum(o => o.PlannedLaborHours)
        };
    }
}
