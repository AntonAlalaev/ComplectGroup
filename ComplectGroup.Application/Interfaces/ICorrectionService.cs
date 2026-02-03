// ComplectGroup.Application/Interfaces/ICorrectionService.cs
namespace ComplectGroup.Application.Interfaces;
using ComplectGroup.Application.DTOs;

public interface ICorrectionService
{
    /// <summary>
    /// Выполнить корректировку пересортицы
    /// </summary>
    Task<CorrectionTransactionDto> CreateCorrectionAsync(
        int oldPartId, 
        int newPartId, 
        int quantity, 
        string notes, 
        CancellationToken ct);
    
    /// <summary>
    /// Получить историю всех корректировок
    /// </summary>
    Task<List<CorrectionTransactionDto>> GetCorrectionHistoryAsync(CancellationToken ct);
}