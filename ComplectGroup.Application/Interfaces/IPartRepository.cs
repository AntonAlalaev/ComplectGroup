using ComplectGroup.Domain.Entities;

namespace ComplectGroup.Application.Interfaces;

/// <summary>
/// Интерфейс репозитория для работы с позициями
/// </summary>
public interface IPartRepository
{
    /// <summary>
    /// Получить позицию по ID
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<Part?> GetByIdAsync(int id, CancellationToken cancellationToken);
    
    /// <summary>
    /// Получить все позиции
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<List<Part>> GetAllAsync(CancellationToken cancellationToken);
}
