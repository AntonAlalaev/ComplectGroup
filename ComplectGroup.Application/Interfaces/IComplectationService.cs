using ComplectGroup.Application.DTOs;

namespace ComplectGroup.Application.Interfaces;

/// <summary>
/// Сервис для работы с комплектациями
/// </summary>
public interface IComplectationService
{
    /// <summary>
    /// Получить комплектацию по id
    /// </summary>
    Task<ComplectationDto> GetByIdAsync(int id, CancellationToken cancellationToken);
    
    /// <summary>
    /// Получить все комплектации
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<List<ComplectationDto>> GetAllAsync(CancellationToken cancellationToken);
    
    /// <summary>
    /// Создать комплектацию
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<ComplectationDto> CreateAsync(CreateComplectationRequest request, CancellationToken cancellationToken);
    
    /// <summary>
    /// Обновить комплектацию по id
    /// </summary>
    /// <param name="id"></param>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task UpdateAsync(int id, UpdateComplectationRequest request, CancellationToken cancellationToken);
    
    /// <summary>
    /// Удалить комплектацию по id
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task DeleteAsync(int id, CancellationToken cancellationToken);

    /// <summary>
    /// Проверить, полностью ли отгружена комплектация
    /// </summary>
    Task<bool> IsFullyShippedAsync(int complectationId, CancellationToken cancellationToken);

    /// <summary>
    /// Отметить как полностью отгруженную
    /// </summary>
    Task MarkAsFullyShippedAsync(int complectationId, CancellationToken cancellationToken);

    /// <summary>
    /// Получить только не полностью отгруженные комплектации
    /// </summary>
    Task<List<ComplectationDto>> GetNotFullyShippedAsync(CancellationToken cancellationToken);
}
