using ComplectGroup.Application.DTOs;

namespace ComplectGroup.Application.Interfaces;

/// <summary>
/// Сервис для работы с бригадами
/// </summary>
public interface IBrigadeService
{
    Task<List<BrigadeDto>> GetAllAsync(CancellationToken cancellationToken);

    Task<BrigadeDto> GetByIdAsync(int id, CancellationToken cancellationToken);

    Task<BrigadeDto> CreateAsync(BrigadeRequest request, CancellationToken cancellationToken);

    Task<BrigadeDto> UpdateAsync(int id, BrigadeRequest request, CancellationToken cancellationToken);

    Task DeleteAsync(int id, CancellationToken cancellationToken);
}
