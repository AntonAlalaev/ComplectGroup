using ComplectGroup.Domain.Entities;

namespace ComplectGroup.Application.Interfaces;

/// <summary>
/// Репозиторий комплектаций
/// </summary>
public interface IComplectationRepository
{
    /// <summary>
    /// Получить комплектацию по ID
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<Complectation?> GetByIdAsync(int id, CancellationToken cancellationToken);
    
    /// <summary>
    /// Получить все комплектации
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<List<Complectation>> GetAllAsync(CancellationToken cancellationToken);
    
    /// <summary>
    /// Добавить комплектацию
    /// </summary>
    /// <param name="complectation"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task AddAsync(Complectation complectation, CancellationToken cancellationToken);
    
    /// <summary>
    /// Обновить комплектацию
    /// </summary>
    /// <param name="complectation"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task UpdateAsync(Complectation complectation, CancellationToken cancellationToken);
    
    /// <summary>
    /// Удалить комплектацию
    /// </summary>
    /// <param name="complectation"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task DeleteAsync(Complectation complectation, CancellationToken cancellationToken);
    
    /// <summary>
    /// Проверить существование комплектации по ID
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<bool> ExistsByIdAsync(int id, CancellationToken cancellationToken);
}
