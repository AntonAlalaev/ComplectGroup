namespace ComplectGroup.Domain.Entities;

/// <summary>
/// Статус комплектации в процессе отгрузки
/// </summary>
public enum ComplectationStatus
{
    /// <summary>
    /// Черновик / В процессе подготовки
    /// </summary>
    Draft = 0,

    /// <summary>
    /// В процессе отгрузки (частично отгружена)
    /// </summary>
    InProgress = 1,

    /// <summary>
    /// Полностью отгружена
    /// </summary>
    FullyShipped = 2,

    /// <summary>
    /// Архивирована
    /// </summary>
    Archived = 3
}
