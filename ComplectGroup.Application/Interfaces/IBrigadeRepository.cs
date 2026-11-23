using ComplectGroup.Domain.Entities.Planning;

namespace ComplectGroup.Application.Interfaces;

/// <summary>
/// Интерфейс репозитория для работы с бригадами
/// </summary>
public interface IBrigadeRepository
{
    Task<List<Brigade>> GetAllAsync(CancellationToken cancellationToken);

    Task<Brigade?> GetByIdAsync(int id, CancellationToken cancellationToken);

    Task AddAsync(Brigade brigade, CancellationToken cancellationToken);

    Task UpdateAsync(Brigade brigade, CancellationToken cancellationToken);

    Task DeleteAsync(Brigade brigade, CancellationToken cancellationToken);

    Task<bool> ExistsByNameAsync(string name, int? excludeId, CancellationToken cancellationToken);

    Task<bool> HasOperationsAsync(int brigadeId, CancellationToken cancellationToken);
}
