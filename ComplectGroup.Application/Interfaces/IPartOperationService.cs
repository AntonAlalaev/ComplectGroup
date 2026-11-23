using ComplectGroup.Application.DTOs;

namespace ComplectGroup.Application.Interfaces;

/// <summary>
/// Сервис для работы с операциями технологических маршрутов деталей
/// </summary>
public interface IPartOperationService
{
    Task<List<PartOperationDto>> GetByPartIdAsync(int partId, CancellationToken cancellationToken);

    Task<PartOperationDto> GetByIdAsync(int id, CancellationToken cancellationToken);

    /// <summary>
    /// Получить сводку по маршрутам деталей с фильтрацией по разделу и названию
    /// </summary>
    Task<List<PartPlanningSummaryDto>> GetPartSummariesAsync(
        int? chapterId,
        string? search,
        CancellationToken cancellationToken);

    Task<PartOperationDto> CreateAsync(PartOperationRequest request, CancellationToken cancellationToken);

    Task<PartOperationDto> UpdateAsync(int id, PartOperationRequest request, CancellationToken cancellationToken);

    Task DeleteAsync(int id, CancellationToken cancellationToken);
}
