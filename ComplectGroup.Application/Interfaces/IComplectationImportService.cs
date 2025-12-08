using ComplectGroup.Application.DTOs;

namespace ComplectGroup.Application.Interfaces;

/// <summary>
/// Сервис для импорта комплектаций из Excel
/// </summary>
public interface IComplectationImportService
{
    /// <summary>
    /// Импортировать комплектации из Excel файла
    /// </summary>
    /// <param name="fileStream">Поток файла Excel</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>Список импортированных комплектаций</returns>
    Task<ComplectationDto> ImportFromExcelAsync(
        Stream fileStream, 
        CancellationToken cancellationToken);
}
