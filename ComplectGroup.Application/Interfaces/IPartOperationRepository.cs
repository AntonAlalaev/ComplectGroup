using ComplectGroup.Domain.Entities.Planning;

namespace ComplectGroup.Application.Interfaces;

/// <summary>
/// Интерфейс репозитория для работы с операциями технологических маршрутов
/// </summary>
public interface IPartOperationRepository
{
    Task<List<PartOperation>> GetAllAsync(CancellationToken cancellationToken);

    Task<List<PartOperation>> GetByPartIdAsync(int partId, CancellationToken cancellationToken);

    Task<PartOperation?> GetByIdAsync(int id, CancellationToken cancellationToken);

    Task AddAsync(PartOperation operation, CancellationToken cancellationToken);

    Task UpdateAsync(PartOperation operation, CancellationToken cancellationToken);

    Task DeleteAsync(PartOperation operation, CancellationToken cancellationToken);

    Task<bool> SequenceExistsAsync(
        int partId,
        int sequenceNumber,
        int? excludeId,
        CancellationToken cancellationToken);
}
