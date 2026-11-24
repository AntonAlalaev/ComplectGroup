using ComplectGroup.Domain.Entities;

namespace ComplectGroup.Domain.Entities.Planning;

/// <summary>
/// Производственный план (агрегатный корень)
/// </summary>
public class ProductionPlan
{
    public int Id { get; set; }

    /// <summary>
    /// Начало периода планирования
    /// </summary>
    public DateTime PeriodStart { get; set; }

    /// <summary>
    /// Конец периода планирования
    /// </summary>
    public DateTime PeriodEnd { get; set; }

    /// <summary>
    /// Статус плана
    /// </summary>
    public ProductionPlanStatus Status { get; set; } = ProductionPlanStatus.Draft;

    /// <summary>
    /// Примечания к плану
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Позиции плана
    /// </summary>
    public ICollection<ProductionPlanItem> Items { get; set; } = new List<ProductionPlanItem>();

    /// <summary>
    /// Добавить позицию в план (только если план не закрыт)
    /// </summary>
    public void AddItem(ProductionPlanItem item)
    {
        if (Status == ProductionPlanStatus.Closed)
            throw new InvalidOperationException("Нельзя добавить позицию в закрытый план");

        Items.Add(item);
    }

    /// <summary>
    /// Удалить позицию из плана (только в черновике)
    /// </summary>
    public void RemoveItem(int itemId)
    {
        if (Status != ProductionPlanStatus.Draft)
            throw new InvalidOperationException("Удалять позиции можно только в черновике плана");

        var item = Items.FirstOrDefault(i => i.Id == itemId);
        if (item != null)
            Items.Remove(item);
    }

    /// <summary>
    /// Утвердить план
    /// </summary>
    public void Approve()
    {
        if (Status != ProductionPlanStatus.Draft)
            throw new InvalidOperationException("Утвердить можно только черновик плана");

        Status = ProductionPlanStatus.Approved;
    }

    /// <summary>
    /// Перевести план в работу
    /// </summary>
    public void Start()
    {
        if (Status != ProductionPlanStatus.Approved)
            throw new InvalidOperationException("В работу можно перевести только утверждённый план");

        Status = ProductionPlanStatus.InProgress;
    }

    /// <summary>
    /// Пересчитать статус плана на основе статусов операций
    /// </summary>
    public void RecalculateStatus()
    {
        var allOperations = Items.SelectMany(i => i.PlannedOperations).ToList();

        if (allOperations.Count == 0)
            return;

        if (allOperations.All(o => o.Status == PlannedOperationStatus.Completed
                                    || o.Status == PlannedOperationStatus.Skipped))
        {
            // Все операции завершены — можно закрыть, но оставляем InProgress до явного Close
            if (Status == ProductionPlanStatus.InProgress)
                Status = ProductionPlanStatus.InProgress;
        }
        else if (allOperations.Any(o => o.Status == PlannedOperationStatus.InProgress
                                        || o.Status == PlannedOperationStatus.Completed))
        {
            if (Status == ProductionPlanStatus.Approved)
                Status = ProductionPlanStatus.InProgress;
        }
    }

    /// <summary>
    /// Закрыть план (все операции должны быть завершены)
    /// </summary>
    public void Close()
    {
        var allOperations = Items.SelectMany(i => i.PlannedOperations).ToList();

        if (allOperations.Any(o => o.Status != PlannedOperationStatus.Completed
                                    && o.Status != PlannedOperationStatus.Skipped))
        {
            throw new InvalidOperationException("Нельзя закрыть план: есть незавершённые операции");
        }

        Status = ProductionPlanStatus.Closed;
    }
}
