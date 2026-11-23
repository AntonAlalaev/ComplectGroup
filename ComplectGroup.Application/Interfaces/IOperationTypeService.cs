using ComplectGroup.Application.DTOs;

namespace ComplectGroup.Application.Interfaces;

/// <summary>
/// Сервис для работы с типами операций
/// </summary>
public interface IOperationTypeService
{
    Task<List<OperationTypeDto>> GetAllAsync(CancellationToken cancellationToken);

    Task<OperationTypeDto> GetByIdAsync(int id, CancellationToken cancellationToken);

    Task<OperationTypeDto> CreateAsync(OperationTypeRequest request, CancellationToken cancellationToken);

    Task<OperationTypeDto> UpdateAsync(int id, OperationTypeRequest request, CancellationToken cancellationToken);

    Task DeleteAsync(int id, CancellationToken cancellationToken);
}
