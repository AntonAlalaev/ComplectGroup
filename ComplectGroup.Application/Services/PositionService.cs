using ComplectGroup.Application.DTOs;
using ComplectGroup.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace ComplectGroup.Application.Services;

/// <summary>
/// Реализация сервиса для работы с позициями
/// </summary>
public class PositionService : IPositionService
{
    private readonly IPositionRepository _repository;
    private readonly ILogger<PositionService> _logger;

    public PositionService(IPositionRepository repository, ILogger<PositionService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<PositionDto?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        var position = await _repository.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Позиция с ID {id} не найдена");

        return MapToDto(position);
    }

    public async Task<List<PositionDto>> GetAllAsync(CancellationToken cancellationToken)
    {
        var positions = await _repository.GetAllAsync(cancellationToken);
        return positions.Select(MapToDto).ToList();
    }

    public async Task<List<PositionDto>> GetByComplectationIdAsync(int complectationId, CancellationToken cancellationToken)
    {
        var positions = await _repository.GetByComplectationIdAsync(complectationId, cancellationToken);
        return positions.Select(MapToDto).ToList();
    }

    private PositionDto MapToDto(Domain.Entities.Position position) =>
        new PositionDto
        {
            Id = position.Id,
            Quantity = position.Quantity,
            Part = new PartDto
            {
                Id = position.Part.Id,
                Name = position.Part.Name,
                Chapter = new ChapterDto
                {
                    Id = position.Part.Chapter.Id,
                    Name = position.Part.Chapter.Name
                }
            }
        };
}
