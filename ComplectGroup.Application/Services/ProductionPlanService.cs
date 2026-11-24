using ComplectGroup.Application.DTOs;
using ComplectGroup.Application.Interfaces;
using ComplectGroup.Domain.Entities;
using ComplectGroup.Domain.Entities.Planning;
using Microsoft.Extensions.Logging;

namespace ComplectGroup.Application.Services;

public class ProductionPlanService : IProductionPlanService
{
    private readonly IProductionPlanRepository _planRepository;
    private readonly IPartRepository _partRepository;
    private readonly IPartOperationRepository _partOperationRepository;
    private readonly ILogger<ProductionPlanService> _logger;

    public ProductionPlanService(
        IProductionPlanRepository planRepository,
        IPartRepository partRepository,
        IPartOperationRepository partOperationRepository,
        ILogger<ProductionPlanService> logger)
    {
        _planRepository = planRepository;
        _partRepository = partRepository;
        _partOperationRepository = partOperationRepository;
        _logger = logger;
    }

    public async Task<List<ProductionPlanDto>> GetAllAsync(CancellationToken cancellationToken)
    {
        var plans = await _planRepository.GetAllAsync(cancellationToken);
        return plans.Select(MapToDto).ToList();
    }

    public async Task<ProductionPlanDto> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        var plan = await _planRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Производственный план с ID {id} не найден");

        return MapToDto(plan);
    }

    public async Task<ProductionPlanDto> CreateAsync(
        DateTime periodStart,
        DateTime periodEnd,
        string? notes,
        bool allowOverlap,
        CancellationToken cancellationToken)
    {
        if (periodStart >= periodEnd)
            throw new ArgumentException("Дата начала периода должна быть меньше даты окончания");

        if (await _planRepository.HasOverlappingActivePlanAsync(periodStart, periodEnd, null, allowOverlap, cancellationToken))
            throw new InvalidOperationException("Период пересекается с другим активным планом");

        var plan = new ProductionPlan
        {
            PeriodStart = periodStart.Date,
            PeriodEnd = periodEnd.Date,
            Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim(),
            Status = ProductionPlanStatus.Draft
        };

        await _planRepository.AddAsync(plan, cancellationToken);
        _logger.LogInformation("Создан производственный план {Id} на период {Start}–{End}", plan.Id, plan.PeriodStart, plan.PeriodEnd);

        return MapToDto(plan);
    }

    public async Task<ProductionPlanDto> AddItemAsync(
        int planId,
        int partId,
        int quantity,
        int? sourceRequirementId,
        CancellationToken cancellationToken)
    {
        if (quantity <= 0)
            throw new ArgumentException("Количество должно быть больше 0");

        var plan = await _planRepository.GetByIdAsync(planId, cancellationToken)
            ?? throw new KeyNotFoundException($"Производственный план с ID {planId} не найден");

        if (plan.Status == ProductionPlanStatus.Closed)
            throw new InvalidOperationException("Нельзя добавить позицию в закрытый план");

        var part = await _partRepository.GetByIdAsync(partId, cancellationToken)
            ?? throw new KeyNotFoundException($"Деталь с ID {partId} не найдена");

        var partOperations = await _partOperationRepository.GetByPartIdAsync(partId, cancellationToken);
        if (partOperations.Count == 0)
            throw new InvalidOperationException("Для детали не задан технологический маршрут");

        var item = new ProductionPlanItem
        {
            PartId = part.Id,
            Quantity = quantity,
            SourceRequirementId = sourceRequirementId
        };

        foreach (var po in partOperations.Where(o => o.IsActive))
        {
            item.PlannedOperations.Add(new PlannedOperation
            {
                PartOperationId = po.Id,
                BrigadeId = po.BrigadeId,
                PlannedHours = quantity * po.PlannedLaborHours,
                Status = PlannedOperationStatus.Pending
            });
        }

        plan.AddItem(item);
        await _planRepository.UpdateAsync(plan, cancellationToken);

        _logger.LogInformation("В план {PlanId} добавлена деталь {PartId} в количестве {Qty}", planId, partId, quantity);

        var updated = await _planRepository.GetByIdAsync(planId, cancellationToken);
        return MapToDto(updated!);
    }

    public async Task<ProductionPlanDto> ReplaceItemsAsync(
        int planId,
        List<int> partIds,
        List<int> quantities,
        CancellationToken cancellationToken)
    {
        if (partIds.Count != quantities.Count)
            throw new ArgumentException("Списки деталей и количеств не совпадают по размеру");

        // Объединяем дубликаты по детали (суммируем количество)
        var merged = new Dictionary<int, int>();
        for (int i = 0; i < partIds.Count; i++)
        {
            if (quantities[i] <= 0)
                continue;
            if (merged.ContainsKey(partIds[i]))
                merged[partIds[i]] += quantities[i];
            else
                merged[partIds[i]] = quantities[i];
        }

        var plan = await _planRepository.GetByIdAsync(planId, cancellationToken)
            ?? throw new KeyNotFoundException($"Производственный план с ID {planId} не найден");

        if (plan.Status != ProductionPlanStatus.Draft)
            throw new InvalidOperationException("Изменять позиции можно только в черновике плана");

        // Удаляем существующие позиции
        var existingItemIds = plan.Items.Select(i => i.Id).ToList();
        foreach (var itemId in existingItemIds)
            plan.RemoveItem(itemId);

        // Добавляем новые
        foreach (var (partId, quantity) in merged)
        {
            if (quantity <= 0)
                continue;

            var part = await _partRepository.GetByIdAsync(partId, cancellationToken);
            if (part == null)
                continue;

            var partOperations = await _partOperationRepository.GetByPartIdAsync(partId, cancellationToken);
            if (partOperations.Count == 0)
                continue;

            var item = new ProductionPlanItem
            {
                PartId = part.Id,
                Quantity = quantity,
                SourceRequirementId = null
            };

            foreach (var po in partOperations.Where(o => o.IsActive))
            {
                item.PlannedOperations.Add(new PlannedOperation
                {
                    PartOperationId = po.Id,
                    BrigadeId = po.BrigadeId,
                    PlannedHours = quantity * po.PlannedLaborHours,
                    Status = PlannedOperationStatus.Pending
                });
            }

            plan.AddItem(item);
        }

        await _planRepository.UpdateAsync(plan, cancellationToken);
        _logger.LogInformation("В план {PlanId} сохранён набор позиций: {Count}", planId, partIds.Count);

        var updated = await _planRepository.GetByIdAsync(planId, cancellationToken);
        return MapToDto(updated!);
    }

    public async Task RemoveItemAsync(int planId, int itemId, CancellationToken cancellationToken)
    {
        var plan = await _planRepository.GetByIdAsync(planId, cancellationToken)
            ?? throw new KeyNotFoundException($"Производственный план с ID {planId} не найден");

        plan.RemoveItem(itemId);
        await _planRepository.UpdateAsync(plan, cancellationToken);
    }

    public async Task ApproveAsync(int planId, CancellationToken cancellationToken)
    {
        var plan = await _planRepository.GetByIdAsync(planId, cancellationToken)
            ?? throw new KeyNotFoundException($"Производственный план с ID {planId} не найден");

        plan.Approve();
        await _planRepository.UpdateAsync(plan, cancellationToken);
    }

    public async Task StartAsync(int planId, CancellationToken cancellationToken)
    {
        var plan = await _planRepository.GetByIdAsync(planId, cancellationToken)
            ?? throw new KeyNotFoundException($"Производственный план с ID {planId} не найден");

        plan.Start();
        await _planRepository.UpdateAsync(plan, cancellationToken);
    }

    public async Task CloseAsync(int planId, CancellationToken cancellationToken)
    {
        var plan = await _planRepository.GetByIdAsync(planId, cancellationToken)
            ?? throw new KeyNotFoundException($"Производственный план с ID {planId} не найден");

        plan.Close();
        await _planRepository.UpdateAsync(plan, cancellationToken);
    }

    public async Task CompleteOperationAsync(int planId, int operationId, decimal actualHours, int? actualQuantity, CancellationToken cancellationToken)
    {
        var plan = await _planRepository.GetByIdAsync(planId, cancellationToken)
            ?? throw new KeyNotFoundException($"Производственный план с ID {planId} не найден");

        var operation = plan.Items
            .SelectMany(i => i.PlannedOperations)
            .FirstOrDefault(o => o.Id == operationId)
            ?? throw new KeyNotFoundException($"Операция с ID {operationId} не найдена в плане");

        operation.Complete(actualHours, actualQuantity);
        plan.RecalculateStatus();
        await _planRepository.UpdateAsync(plan, cancellationToken);
    }

    public async Task SkipOperationAsync(int planId, int operationId, string reason, CancellationToken cancellationToken)
    {
        var plan = await _planRepository.GetByIdAsync(planId, cancellationToken)
            ?? throw new KeyNotFoundException($"Производственный план с ID {planId} не найден");

        var operation = plan.Items
            .SelectMany(i => i.PlannedOperations)
            .FirstOrDefault(o => o.Id == operationId)
            ?? throw new KeyNotFoundException($"Операция с ID {operationId} не найдена в плане");

        operation.Skip(reason);
        plan.RecalculateStatus();
        await _planRepository.UpdateAsync(plan, cancellationToken);
    }

    public async Task<List<BrigadeWorkloadDto>> GetWorkloadByBrigadeAsync(
        DateTime periodStart,
        DateTime periodEnd,
        CancellationToken cancellationToken)
    {
        var plans = await _planRepository.GetAllAsync(cancellationToken);

        var relevantPlans = plans.Where(p =>
            p.PeriodStart < periodEnd.Date && p.PeriodEnd > periodStart.Date).ToList();

        var workload = new Dictionary<int, BrigadeWorkloadDto>();

        foreach (var plan in relevantPlans)
        {
            foreach (var item in plan.Items)
            {
                foreach (var op in item.PlannedOperations)
                {
                    if (!workload.TryGetValue(op.BrigadeId, out var dto))
                    {
                        dto = new BrigadeWorkloadDto
                        {
                            BrigadeId = op.BrigadeId,
                            BrigadeName = op.Brigade?.Name ?? string.Empty
                        };
                        workload[op.BrigadeId] = dto;
                    }

                    dto.TotalPlannedHours += op.PlannedHours;
                    dto.TotalActualHours += op.ActualHours ?? 0;
                    dto.OperationsCount++;
                    if (op.Status == PlannedOperationStatus.Completed)
                        dto.CompletedCount++;
                }
            }
        }

        return workload.Values.OrderBy(w => w.BrigadeName).ToList();
    }

    private static ProductionPlanDto MapToDto(ProductionPlan plan)
    {
        var items = plan.Items.Select(i => new ProductionPlanItemDto
        {
            Id = i.Id,
            PartId = i.PartId,
            PartName = i.Part?.Name ?? string.Empty,
            ChapterName = i.Part?.Chapter?.Name ?? string.Empty,
            Quantity = i.Quantity,
            SourceRequirementId = i.SourceRequirementId,
            PlannedOperations = i.PlannedOperations.Select(o => new PlannedOperationDto
            {
                Id = o.Id,
                ProductionPlanItemId = o.ProductionPlanItemId,
                PartOperationId = o.PartOperationId,
                OperationTypeCode = o.PartOperation?.OperationType?.Code ?? string.Empty,
                OperationTypeName = o.PartOperation?.OperationType?.Name ?? string.Empty,
                BrigadeId = o.BrigadeId,
                BrigadeName = o.Brigade?.Name ?? string.Empty,
                PlannedHours = o.PlannedHours,
                Status = o.Status.ToString(),
                StatusCode = (int)o.Status,
                ActualHours = o.ActualHours,
                ActualQuantity = o.ActualQuantity,
                CompletedAt = o.CompletedAt,
                SkipReason = o.SkipReason
            }).OrderBy(o => o.Id).ToList(),
            TotalPlannedHours = i.PlannedOperations.Sum(o => o.PlannedHours)
        }).ToList();

        var allOps = items.SelectMany(i => i.PlannedOperations).ToList();

        return new ProductionPlanDto
        {
            Id = plan.Id,
            PeriodStart = plan.PeriodStart,
            PeriodEnd = plan.PeriodEnd,
            Status = plan.Status.ToString(),
            StatusCode = (int)plan.Status,
            Notes = plan.Notes,
            Items = items,
            TotalPlannedHours = allOps.Sum(o => o.PlannedHours),
            TotalOperations = allOps.Count,
            CompletedOperations = allOps.Count(o => o.StatusCode == (int)PlannedOperationStatus.Completed)
        };
    }
}
