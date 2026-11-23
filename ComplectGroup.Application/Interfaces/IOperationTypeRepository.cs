using ComplectGroup.Domain.Entities.Planning;

namespace ComplectGroup.Application.Interfaces;

/// <summary>
/// Интерфейс репозитория для работы с типами операций
/// </summary>
public interface IOperationTypeRepository
{
    Task<List<OperationType>> GetAllAsync(CancellationToken cancellationToken);

    Task<OperationType?> GetByIdAsync(int id, CancellationToken cancellationToken);

    Task AddAsync(OperationType operationType, CancellationToken cancellationToken);

    Task UpdateAsync(OperationType operationType, CancellationToken cancellationToken);

    Task DeleteAsync(OperationType operationType, CancellationToken cancellationToken);

    Task<bool> ExistsByCodeAsync(string code, int? excludeId, CancellationToken cancellationToken);

    Task<bool> HasOperationsAsync(int operationTypeId, CancellationToken cancellationToken);
}
