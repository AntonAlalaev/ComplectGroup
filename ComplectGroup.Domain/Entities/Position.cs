namespace ComplectGroup.Domain.Entities;

public class Position
{
    /// <summary>
    /// Идентификатор позиции в комплектации
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Внешний ключ на деталь
    /// </summary>
    public int PartId { get; set; }
   
    /// <summary>
    /// Позиция в комплектации
    /// </summary>
    public Part Part { get; set; }

    /// <summary>
    /// Количество позиций в комплектации
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// ID комплектации, которой принадлежит позиция
    /// </summary>
    public int ComplectationId { get; set; }
}