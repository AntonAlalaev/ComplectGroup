using System.ComponentModel.DataAnnotations;

namespace ComplectGroup.Application.DTOs;

/// <summary>
/// Модель комплектации для передачи данных в API
/// </summary>
public class ComplectationDto
{
    /// <summary>
    /// Уникальный идентификатор комплектации
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Номер комплектации
    /// </summary>
    public string Number { get; set; } = string.Empty;

    /// <summary>
    /// Менеджер - ответственный за комплектацию
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
    /// Дата создания комплектации
    /// </summary>
    public DateOnly? CreatedDate { get; set; }

    /// <summary>
    /// Условия отгрузки
    /// </summary>
    public string ShippingTerms { get; set; } = string.Empty;

    /// <summary>
    /// Общий вес
    /// </summary>
    public double TotalWeight { get; set; }

    /// <summary>
    /// Общий объем
    /// </summary>
    public double TotalVolume { get; set; }

    /// <summary>
    /// Статус отгрузки
    /// </summary>
    public int Status { get; set; } // int потому что enum сериализуется в int

    /// <summary>
    /// Флаг: игнорировать комплектацию в сводной таблице
    /// </summary>
    public bool IsIgnored { get; set; }

    /// <summary>
    /// Дата полной отгрузки
    /// </summary>
    public DateTime? FullyShippedDate { get; set; }

    /// <summary>
    /// Позиции комплектации
    /// </summary>
    public List<PositionDto> Positions { get; set; } = [];
}
