using ComplectGroup.Application.DTOs;

namespace ComplectGroup.Application.Interfaces;

/// <summary>
/// Сервис для импорта комплектаций из Excel
/// </summary>
public interface IComplectationImportService
{
    /// <summary>
    /// Импортировать одну комплектацию из Excel файла
    /// </summary>
    /// <param name="fileStream">Поток файла Excel</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>Импортированная комплектация</returns>
    Task<ComplectationDto> ImportFromExcelAsync(
        Stream fileStream,
        CancellationToken cancellationToken);

    /// <summary>
    /// Импортировать несколько комплектаций из Excel файла
    /// </summary>
    /// <param name="fileStream">Поток файла Excel</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>Список импортированных комплектаций</returns>
    Task<List<ComplectationDto>> ImportMultipleFromExcelAsync(
        Stream fileStream,
        CancellationToken cancellationToken);

    /// <summary>
    /// Проверить Excel файл на корректность (без загрузки в базу)
    /// </summary>
    /// <param name="fileStream">Поток файла Excel</param>
    /// <param name="fileName">Имя файла для отображения</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>Список результатов проверки комплектаций</returns>
    Task<List<ComplectationValidationResult>> ValidateExcelFileAsync(
        Stream fileStream,
        string fileName,
        CancellationToken cancellationToken);
}
