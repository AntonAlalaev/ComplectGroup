using System.ComponentModel.DataAnnotations;

namespace ComplectGroup.Application.DTOs;

/// <summary>
/// Запрос на создание комплектации
/// </summary>
public class CreateComplectationRequest
{
    /// <summary>
    /// Номер комплектации (обязательно)
    /// </summary>
    [Required(ErrorMessage = "Необходим номер комплектации или ее обозначение")]
    public string Number { get; set; } = string.Empty;

    /// <summary>
    /// Менеджер — ответственный
    /// </summary>
    public string Manager { get; set; } = string.Empty;

    /// <summary>
    /// Адрес отгрузки
    /// </summary>
    public string Address { get; set; } = string.Empty;

    /// <summary>
    /// Заказчик
    /// </summary>
    public string Customer { get; set; } = string.Empty;

    /// <summary>
    /// Дата отгрузки
    /// </summary>
    public DateOnly ShippingDate { get; set; }

    /// <summary>
    /// Дата создания (если не указана — будет установлена сервером)
    /// </summary>
    public DateOnly? CreatedDate { get; set; }

    /// <summary>
    /// Условия отгрузки
    /// </summary>
    public string ShippingTerms { get; set; } = string.Empty;

    /// <summary>
    /// Общий вес
    /// </summary>
    [Range(0, double.MaxValue, ErrorMessage ="Общий вес не может быть отрицательным")]
    public double TotalWeight { get; set; }
    
    /// <summary>
    /// Общий объем
    /// </summary>
    [Range(0, double.MaxValue, ErrorMessage ="Общий объем не может быть отрицательным")]
    public double TotalVolume { get; set; }

    /// <summary>
    /// Позиции комплектации
    /// </summary>
    public List<CreatePositionRequest> Positions { get; set; } =[];
}
