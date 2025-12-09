namespace ComplectGroup.Application.DTOs;

/// <summary>
/// Запрос на обновление комплектации
/// </summary>
public class UpdateComplectationRequest
{
    /// <summary>
    /// Номер комплектации
    /// </summary>
    public string? Number { get; set; }

    /// <summary>
    /// Менеджер
    /// </summary>
    public string? Manager { get; set; }

    /// <summary>
    /// Адрес отгрузки
    /// </summary>
    public string? Address { get; set; }

    /// <summary>
    /// Заказчик
    /// </summary>
    public string? Customer { get; set; }

    /// <summary>
    /// Дата отгрузки
    /// </summary>
    public DateOnly? ShippingDate { get; set; }

    /// <summary>
    /// Условия отгрузки
    /// </summary>
    public string? ShippingTerms { get; set; }

    public double? TotalWeight { get; set; }     
    public double? TotalVolume { get; set; } 
    
    /// <summary>
    /// Позиции (опционально — можно обновлять частично)
    /// </summary>
    public List<UpdatePositionRequest>? Positions { get; set; }
}
