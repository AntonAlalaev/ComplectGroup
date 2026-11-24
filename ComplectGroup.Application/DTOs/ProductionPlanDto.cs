using ComplectGroup.Domain.Entities;

namespace ComplectGroup.Application.DTOs;

public class PlannedOperationDto
{
    public int Id { get; set; }
    public int ProductionPlanItemId { get; set; }
    public int PartOperationId { get; set; }
    public string OperationTypeCode { get; set; } = string.Empty;
    public string OperationTypeName { get; set; } = string.Empty;
    public int BrigadeId { get; set; }
    public string BrigadeName { get; set; } = string.Empty;
    public decimal PlannedHours { get; set; }
    public string Status { get; set; } = string.Empty;
    public int StatusCode { get; set; }
    public decimal? ActualHours { get; set; }
    public int? ActualQuantity { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? SkipReason { get; set; }
}

public class ProductionPlanItemDto
{
    public int Id { get; set; }
    public int PartId { get; set; }
    public string PartName { get; set; } = string.Empty;
    public string ChapterName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public int? SourceRequirementId { get; set; }
    public List<PlannedOperationDto> PlannedOperations { get; set; } = new();
    public decimal TotalPlannedHours { get; set; }
}

public class ProductionPlanDto
{
    public int Id { get; set; }
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public string Status { get; set; } = string.Empty;
    public int StatusCode { get; set; }
    public string? Notes { get; set; }
    public List<ProductionPlanItemDto> Items { get; set; } = new();
    public decimal TotalPlannedHours { get; set; }
    public int TotalOperations { get; set; }
    public int CompletedOperations { get; set; }
}

public class BrigadeWorkloadDto
{
    public int BrigadeId { get; set; }
    public string BrigadeName { get; set; } = string.Empty;
    public decimal TotalPlannedHours { get; set; }
    public decimal TotalActualHours { get; set; }
    public int OperationsCount { get; set; }
    public int CompletedCount { get; set; }
}
