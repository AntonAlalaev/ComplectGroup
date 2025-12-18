namespace ComplectGroup.Web.Models;

/// <summary>
/// ViewModel для отчёта по деталям за дату
/// </summary>
public class PartsReportViewModel
{
    public DateOnly? From { get; set; }
    public DateOnly? To { get; set; }

    public string SelectedChapter { get; set; } = "*";
    public List<string> AvailableChapters { get; set; } = new();
    
    /// <summary>
    /// Колонки комплектаций
    /// </summary>
    public List<PartsReportComplectationColumn> ComplectationColumns { get; set; } = new();
    
    /// <summary>
    /// Строки отчёта с данными по деталям
    /// </summary>    
    public List<PartsReportRow> Rows { get; set; } = new();
}


public class PartsReportComplectationColumn
{
    public int ComplectationId { get; set; }

    public DateOnly ShippingDate { get; set; }
    public string Number { get; set; } = string.Empty;
    public double TotalWeight { get; set; }
    public double TotalVolume { get; set; }
}

public class PartsReportRow
{
    public string Chapter { get; set; } = string.Empty;
    public string PartName { get; set; } = string.Empty;
    
    // Итого теперь будет пересчитан как сумма по комплектациям к отгрузке
    public int TotalQuantity { get; set; }

    /// <summary>
    /// Количество деталей, доступных на складе
    /// </summary>
    public int WarehouseQuantity { get; set; }

    /// <summary>
    /// Дефицит = Итого - На складе
    /// </summary>
    public int DeficitQuantity => Math.Max(0, TotalQuantity - WarehouseQuantity);    
    
    // Требуемое по каждой комплектации
    public Dictionary<int, int> QuantitiesByComplectation { get; set; } = new();
    public Dictionary<int, int> ShippedByComplectation { get; set; } = new();
}

