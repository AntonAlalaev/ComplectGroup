namespace ComplectGroup.Application.DTOs;

/// <summary>
/// Результат проверки комплектации из Excel файла
/// </summary>
public class ComplectationValidationResult
{
    /// <summary>
    /// Имя файла, из которого импортирована комплектация
    /// </summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// Имя листа в Excel файле
    /// </summary>
    public string WorksheetName { get; set; } = string.Empty;

    /// <summary>
    /// Номер комплектации
    /// </summary>
    public string Number { get; set; } = string.Empty;

    /// <summary>
    /// Менеджер
    /// </summary>
    public string Manager { get; set; } = string.Empty;

    /// <summary>
    /// Заказчик
    /// </summary>
    public string Customer { get; set; } = string.Empty;

    /// <summary>
    /// Адрес отгрузки
    /// </summary>
    public string Address { get; set; } = string.Empty;

    /// <summary>
    /// Дата отгрузки
    /// </summary>
    public DateOnly ShippingDate { get; set; }

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
    /// Позиции комплектации
    /// </summary>
    public List<ValidationPositionDto> Positions { get; set; } = [];

    /// <summary>
    /// Ошибки валидации
    /// </summary>
    public List<string> Errors { get; set; } = [];

    /// <summary>
    /// Предупреждения
    /// </summary>
    public List<string> Warnings { get; set; } = [];

    /// <summary>
    /// Флаг: комплектация валидна
    /// </summary>
    public bool IsValid => !Errors.Any();
}

/// <summary>
/// Позиция комплектации для проверки
/// </summary>
public class ValidationPositionDto
{
    /// <summary>
    /// Раздел/категория
    /// </summary>
    public string Chapter { get; set; } = string.Empty;

    /// <summary>
    /// Наименование детали
    /// </summary>
    public string PartName { get; set; } = string.Empty;

    /// <summary>
    /// Количество
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// Флаг: деталь найдена в базе
    /// </summary>
    public bool PartExists { get; set; }

    /// <summary>
    /// Флаг: раздел найден в базе
    /// </summary>
    public bool ChapterExists { get; set; }

    /// <summary>
    /// ID существующей детали (если найдена)
    /// </summary>
    public int? PartId { get; set; }

    /// <summary>
    /// ID существующего раздела (если найден)
    /// </summary>
    public int? ChapterId { get; set; }
}
