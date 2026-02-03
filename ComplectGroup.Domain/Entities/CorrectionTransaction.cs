// ComplectGroup.Domain/Entities/CorrectionTransaction.cs
namespace ComplectGroup.Domain.Entities;

/// <summary>
/// Транзакция корректировки пересортицы
/// </summary>
public class CorrectionTransaction
{
    public int Id { get; set; }
    
    /// <summary>
    /// Уникальный номер корректировки (например: CORR-2026-001)
    /// </summary>
    public string CorrectionNumber { get; set; } = string.Empty;
    
    /// <summary>
    /// Старая деталь (с которой списываем)
    /// </summary>
    public int OldPartId { get; set; }
    public required Part OldPart { get; set; }
    
    /// <summary>
    /// Новая деталь (которую приходуем)
    /// </summary>
    public int NewPartId { get; set; }
    public required Part NewPart { get; set; }
    
    /// <summary>
    /// Количество для корректировки
    /// </summary>
    public int Quantity { get; set; }
    
    /// <summary>
    /// Дата корректировки
    /// </summary>
    public DateTime CorrectionDate { get; set; }
    
    /// <summary>
    /// Примечания
    /// </summary>
    public string Notes { get; set; } = string.Empty;
    
    /// <summary>
    /// Кто выполнил корректировку
    /// </summary>
    public string CreatedBy { get; set; } = string.Empty;
}