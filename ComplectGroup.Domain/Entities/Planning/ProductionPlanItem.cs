using ComplectGroup.Domain.Entities;

namespace ComplectGroup.Domain.Entities.Planning;

/// <summary>
/// Позиция производственного плана (деталь, которую нужно изготовить)
/// </summary>
public class ProductionPlanItem
{
    public int Id { get; set; }

    public int ProductionPlanId { get; set; }

    public ProductionPlan ProductionPlan { get; set; } = null!;

    public int PartId { get; set; }

    public Part Part { get; set; } = null!;

    /// <summary>
    /// Количество единиц детали, планируемых к изготовлению
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// Ссылка на строку потребности / комплектации, из которой пришла позиция
    /// </summary>
    public int? SourceRequirementId { get; set; }

    /// <summary>
    /// Плановые операции для этой позиции
    /// </summary>
    public ICollection<PlannedOperation> PlannedOperations { get; set; } = new List<PlannedOperation>();
}
