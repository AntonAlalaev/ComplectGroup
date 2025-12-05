using System.ComponentModel.DataAnnotations;

namespace ComplectGroup.Domain.Entities;

/// <summary>
/// Комплектация - сушность
/// </summary>
public class Complectation
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
    /// Менеджер - ответственный за комплектацию, сотрудник
    /// </summary>
    public string Manager { get; set; } = string.Empty;

    /// <summary>
    /// Адрес отгрузки комплектации
    /// </summary>
    public string Address { get; set; } = string.Empty;

    /// <summary>
    /// Наименование заказчика
    /// </summary>
    public string Customer { get; set; } = string.Empty;

    /// <summary>
    /// Дата отгрузки комплектации
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
    /// Суммарный вес комплектации
    /// </summary>
    public double TotalWeight{ get; set; }

    /// <summary>
    /// Суммарный объем комплектации
    /// </summary>
    public double TotalVolume { get; set; }

    /// <summary>
    /// Позиции комплектации
    /// </summary>
    public List<Position> Positions { get; set; } = [];
}