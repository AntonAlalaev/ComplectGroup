public class PartsReportViewModel
{
    public DateOnly? From { get; set; }
    public DateOnly? To { get; set; }

    public List<PartsReportComplectationColumn> ComplectationColumns { get; set; } = new();
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
    public int TotalQuantity { get; set; }

    public Dictionary<int, int> QuantitiesByComplectation { get; set; } = new();
}

