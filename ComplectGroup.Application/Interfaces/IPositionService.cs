using ComplectGroup.Application.DTOs;

namespace ComplectGroup.Application.Interfaces;

/// <summary>
/// Сервис для работы с позициями комплектации
/// </summary>
public interface IPositionService
{
    /// <summary>
    /// Получить позицию по ID
    /// </summary>
    Task<PositionDto?> GetByIdAsync(int id, CancellationToken cancellationToken);

    /// <summary>
    /// Получить все позиции
    /// </summary>
    Task<List<PositionDto>> GetAllAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Получить позиции комплектации
    /// </summary>
    Task<List<PositionDto>> GetByComplectationIdAsync(int complectationId, CancellationToken cancellationToken);
}
