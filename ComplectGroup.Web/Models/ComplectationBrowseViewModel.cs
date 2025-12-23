using ComplectGroup.Application.DTOs;

namespace ComplectGroup.Web.Models;

public class ComplectationBrowseViewModel
{
    /// <summary>
    /// Список комплектаций
    /// </summary>
    public List<ComplectationDto> Complectations { get; set; } = new();
    
    /// <summary>
    /// Выбранная комплектация
    /// </summary>
    public ComplectationDto? SelectedComplectation { get; set; }
    
    /// <summary>
    /// Информация по позициям (с остатками на складе и отгрузками)
    /// </summary>
    public List<PositionDetailRow> PositionDetails { get; set; } = new();
    
    /// <summary>
    /// Флаг фильтра: показывать только позиции с дефицитом
    /// </summary>
    public bool ShowOnlyDeficit { get; set; } = false;
}

/// <summary>
/// Расширенная информация по позиции
/// </summary>
public class PositionDetailRow
{
    public int PositionId { get; set; }
    public int PartId { get; set; }
    public string Chapter { get; set; } = string.Empty;
    public string PartName { get; set; } = string.Empty;
    public int RequiredQuantity { get; set; }
    public int WarehouseQuantity { get; set; }
    public int ShippedQuantity { get; set; }
    
    // Вычисляемые свойства
    public int RemainingToShip => RequiredQuantity - ShippedQuantity;
    public int Deficit => Math.Max(0, RequiredQuantity - WarehouseQuantity - ShippedQuantity);
}

