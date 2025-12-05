namespace ComplectGroup.Domain.Entities;

public class Position
{
    /// <summary>
    /// Идентификатор позиции в комплектации
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Позиция в комплектации
    /// </summary>
    public required Part Part { get; set; }

    /// <summary>
    /// Количество позиций в комплектации
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// ID комплектации, которой принадлежит позиция
    /// </summary>
    public int ComplectationId { get; set; }
}