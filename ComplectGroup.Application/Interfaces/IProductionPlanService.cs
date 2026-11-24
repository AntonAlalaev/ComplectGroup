using ComplectGroup.Application.DTOs;
using ComplectGroup.Domain.Entities.Planning;

namespace ComplectGroup.Application.Interfaces;

public interface IProductionPlanService
{
    Task<List<ProductionPlanDto>> GetAllAsync(CancellationToken cancellationToken);

    Task<ProductionPlanDto> GetByIdAsync(int id, CancellationToken cancellationToken);

    Task<ProductionPlanDto> CreateAsync(
        DateTime periodStart,
        DateTime periodEnd,
        string? notes,
        bool allowOverlap,
        CancellationToken cancellationToken);

    Task<ProductionPlanDto> AddItemAsync(
        int planId,
        int partId,
        int quantity,
        int? sourceRequirementId,
        CancellationToken cancellationToken);

    Task<ProductionPlanDto> ReplaceItemsAsync(
        int planId,
        List<int> partIds,
        List<int> quantities,
        CancellationToken cancellationToken);

    Task RemoveItemAsync(int planId, int itemId, CancellationToken cancellationToken);

    Task ApproveAsync(int planId, CancellationToken cancellationToken);

    Task StartAsync(int planId, CancellationToken cancellationToken);

    Task CloseAsync(int planId, CancellationToken cancellationToken);

    Task CompleteOperationAsync(int planId, int operationId, decimal actualHours, int? actualQuantity, CancellationToken cancellationToken);

    Task SkipOperationAsync(int planId, int operationId, string reason, CancellationToken cancellationToken);

    Task<List<BrigadeWorkloadDto>> GetWorkloadByBrigadeAsync(
        DateTime periodStart,
        DateTime periodEnd,
        CancellationToken cancellationToken);
}
