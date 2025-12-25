using System.Globalization;
using ComplectGroup.Application.DTOs;
using ComplectGroup.Application.Interfaces;
using Microsoft.Extensions.Logging;
using OfficeOpenXml;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System;

namespace ComplectGroup.Application.Services;

/// <summary>
/// Реализация сервиса импорта комплектаций из Excel
/// </summary>
public class ComplectationImportService : IComplectationImportService
{
    private readonly IPartRepository _partRepository;
    private readonly ILogger<ComplectationImportService> _logger;
    private readonly IPartService _partService;  
    private readonly IChapterService _chapterService;


    public ComplectationImportService(
        IPartRepository partRepository,
        IPartService partService,
        IChapterService chapterService,
        ILogger<ComplectationImportService> logger)
    {
        _partRepository = partRepository;
        _partService = partService;
        _chapterService = chapterService;
        _logger = logger;
    }

    /// <summary>
    /// Импорт комплектаций из Excel
    /// </summary>
    /// <param name="fileStream">поток файла</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<ComplectationDto> ImportFromExcelAsync(
        Stream fileStream,
        CancellationToken cancellationToken)
    {
        ComplectationDto importedComplectations = new();

        try
        {
            using (ExcelPackage package = new(fileStream))
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets[0] ?? throw new Exception("Excel файл не содержит листов");
                int column_count = worksheet.Dimension.End.Column;
                int row_count = worksheet.Dimension.End.Row;                

                // номер комплектации
                string compl_number = CutStringAfterSymbol(worksheet.Cells[4, 1].Value?.ToString(), "№");
                if (compl_number == string.Empty)
                {
                    throw new Exception("Данный файл не содержит номер комплектации");
                }
                importedComplectations.Number = compl_number;

                // название проекта
                string  prj_name = worksheet.Cells[5, 3].Value?.ToString() ?? string.Empty;
                if (prj_name == string.Empty)
                {
                    prj_name = "не задано";
                }
                //importedComplectations.Нет раздела пока

                // заказчик
                string cust_name = worksheet.Cells[6, 3].Value?.ToString() ?? string.Empty;
                if (cust_name == string.Empty)
                {
                    cust_name = "заказчик не указан";
                }
                importedComplectations.Customer = cust_name;
                
                // адрес
                string address = worksheet.Cells[7, 3].Value?.ToString() ?? string.Empty;
                if (address == string.Empty)
                {
                    address = "адрес не указан";
                }
                importedComplectations.Address = address;

                // менеджер
                string manager = worksheet.Cells[8, 3].Value?.ToString() ?? string.Empty;
                if (manager == string.Empty)
                {
                    manager = "менеджер не указан";
                }
                importedComplectations.Manager = manager;

                // дата отгрузки
                string ship_date = worksheet.Cells[10, 3].Value?.ToString() ?? string.Empty;
                if (ship_date == string.Empty)
                {
                    throw new Exception("Дата отгрузки не указана!!");
                }
                // разбираем формат даты
                DateOnly? ship_date_format  = ParseRussianDate(ship_date);
                if (!ship_date_format.HasValue)
                {
                    throw new Exception("Дата отгрузки указана не верно, не могу распознать дату!!");                    
                }
                importedComplectations.ShippingDate = ship_date_format.Value;


                // дата создания
                string created_date = worksheet.Cells[1, 8].Value?.ToString() ?? string.Empty;

                // условия отгрузки
                string ship_terms = worksheet.Cells[11, 3].Value?.ToString() ?? string.Empty;
                if (ship_terms == string.Empty)
                {
                    ship_terms = "не указаны";
                }
                importedComplectations.ShippingTerms = ship_terms;

                // словарь для хранения разделов и позиций
                Dictionary<string,Dictionary<string,int>> Elements = [];
                string directory = "";
                float total_weight = 0, total_volume = 0;
                // запускаем цикл, бежиим по строкам
                for (int row = 12; row <= row_count; row++)
                {
                    
                    // делаем из строки словарь, 
                    Dictionary<int, string> row_dict = [];
                    // бежим по столбцам
                    for (int col =1; col <= column_count; col++)
                    {
                        string? cellValue = worksheet.Cells[row, col].Value?.ToString();
                        if (!string.IsNullOrEmpty(cellValue))
                        {                            
                            row_dict.Add(col, cellValue.Trim());
                        }
                    }
                    
                    // если словарь пустой, то бежим дальше
                    if (row_dict.Count == 0)
                        continue;

                    // если словарь не пустой, то начинаем проверять его на раздел комплектации
                    Tuple<bool, string> check_dir = check_directory(row_dict);
                    if (check_dir.Item1)
                    {
                        directory = check_dir.Item2;
                        continue;
                    }

                    // проверяем позицию
                    Tuple<bool, string, int> check_pos = check_position(row_dict);
                    if (check_pos.Item1)
                    {
                        // если раздел уже есть
                        if (Elements.ContainsKey(directory))
                        {
                            // если позиция уже есть в разделе
                            if (Elements[directory].ContainsKey(check_pos.Item2))
                            {
                                // прибавляем значение
                                Elements[directory][check_pos.Item2] += check_pos.Item3;
                            }
                            // если позиции еще нет в разделе
                            else
                            {
                                Elements[directory].Add(check_pos.Item2, check_pos.Item3);
                            }
                        }
                        // если раздела еще нет
                        else
                        {
                            // создаем позицию
                            Dictionary<string, int> to_add = new Dictionary<string, int>
                            {
                                { check_pos.Item2, check_pos.Item3 }
                            };
                            // добавляем ключ - раздел, значение - позиция
                            Elements.Add(directory, to_add );
                        }
                    }

                    // проверяем вес
                    Tuple<bool,float> check_wei = cheсk_weight(row_dict);
                    if (check_wei.Item1) 
                    {
                        total_weight += check_wei.Item2;
                    }

                    // проверяем объем
                    Tuple<bool, float> check_vol = cheсk_volume(row_dict);
                    if (check_vol.Item1)
                    {
                        total_volume += check_vol.Item2;
                    }
                } // конец цикла по строкам
                
                // Пора, записать все данные в объект DTO
                // В итоге у меня есть Dictionary<string,Dictionary<string,int>> Elements = []; Где ключ это раздел, значение это словарь позиций, который состоит из Наименования детали и количества

                // Получаем все детали из БД для маппинга имя -> PartDto
                var allParts = await _partRepository.GetAllAsync(cancellationToken);
                var allChapters = await _chapterService.GetAllAsync(cancellationToken);

                // Маппинги с преобразованием Entity -> DTO
                var chapterNameToChapterMap = allChapters.ToDictionary(c => c.Name.ToLower(), c => c);

                // ✅ ИСПРАВЛЕННЫЙ маппинг
                var partNameToPartMap = allParts.ToDictionary(
                    p => p.Name.ToLower(), 
                    p => new PartDto 
                    { 
                        Id = p.Id, 
                        Name = p.Name,
                        Chapter = new ChapterDto 
                        { 
                            Id = p.Chapter.Id,
                            Name = p.Chapter.Name
                        }
                    });

                var positions = new List<PositionDto>();
                var createdChapters = new HashSet<string>();
                var createdParts = new HashSet<string>();

                foreach (var chapter in Elements)
                {
                    string chapterName = chapter.Key;
                    var positionsInChapter = chapter.Value;

                    // Проверяем, есть ли такой раздел в БД
                    ChapterDto? chapterDto = null;
                    if (!chapterNameToChapterMap.TryGetValue(chapterName.ToLower(), out chapterDto))
                    {
                        // Раздела нет, создаём его
                        try
                        {
                            _logger.LogInformation($"📝 Раздел '{chapterName}' не найден, создаю новый...");
                            chapterDto = await _chapterService.CreateAsync(chapterName, cancellationToken);
                            chapterNameToChapterMap.Add(chapterName.ToLower(), chapterDto);
                            createdChapters.Add(chapterName);
                            _logger.LogInformation($"✅ Раздел '{chapterName}' успешно создан (ID: {chapterDto.Id})");
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, $"❌ Ошибка при создании раздела '{chapterName}'");
                            continue;
                        }
                    }

                    // Обрабатываем позиции в этом разделе
                    foreach (var positionItem in positionsInChapter)
                    {
                        string partName = positionItem.Key;
                        int quantity = positionItem.Value;

                        PartDto? partDto = null;
                        
                        // ✅ ИСПРАВЛЕННАЯ строка: ищем по PartDto (из маппинга)
                        if (!partNameToPartMap.TryGetValue(partName.ToLower(), out partDto))
                        {
                            // Детали нет, создаём её
                            try
                            {
                                _logger.LogInformation($"📝 Деталь '{partName}' в разделе '{chapterName}' не найдена, создаю новую...");
                                partDto = await _partService.CreateAsync(partName, chapterDto.Id, cancellationToken);
                                
                                // ✅ ИСПРАВЛЕННАЯ строка: добавляем PartDto в маппинг
                                partNameToPartMap.Add(partName.ToLower(), partDto);
                                createdParts.Add($"{chapterName}::{partName}");
                                _logger.LogInformation($"✅ Деталь '{partName}' успешно создана (ID: {partDto.Id})");
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, $"❌ Ошибка при создании детали '{partName}' в разделе '{chapterName}'");
                                continue;
                            }
                        }

                        // Создаём позицию
                        try
                        {
                            var position = new PositionDto
                            {
                                Part = partDto,
                                Quantity = quantity
                            };

                            positions.Add(position);
                            _logger.LogInformation($"✅ Добавлена позиция: [{chapterName}] {partName} x{quantity}");
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, $"❌ Ошибка при добавлении позиции '{partName}'");
                        }
                    }
                }
                // Логируем итоги импорта
                if (positions.Count == 0)
                {
                    _logger.LogWarning("⚠️ Не найдено ни одной валидной позиции");
                    throw new Exception("Не удалось импортировать ни одну позицию. Проверьте файл.");
                }

                _logger.LogInformation($"📊 ИТОГИ ИМПОРТА:");
                _logger.LogInformation($"   • Создано разделов: {createdChapters.Count}");
                _logger.LogInformation($"   • Создано деталей: {createdParts.Count}");
                _logger.LogInformation($"   • Добавлено позиций: {positions.Count}");

                // Добавляем позиции в результирующий объект
                importedComplectations.Positions = positions;
                importedComplectations.TotalWeight = total_weight;
                importedComplectations.TotalVolume = total_volume;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при импорте из Excel");
            throw;
        }

        return importedComplectations;
    }

    /// <summary>
    /// Cuts the input string at the specified symbol and returns the part after the symbol.
    /// If the symbol is not found, an empty string is returned.
    /// </summary>
    /// <param name="originalString">The string to be cut.</param>
    /// <param name="cutSymbol">The symbol at which to cut the string.</param>
    /// <returns>The part of the string after the cut symbol, or an empty string if not found.</returns>
    private static string CutStringAfterSymbol(string? originalString, string cutSymbol)
    {
        if (string.IsNullOrEmpty(originalString) || string.IsNullOrEmpty(cutSymbol))
        {
            return string.Empty; // Return empty if either string is null or empty
        }

        int symbolIndex = originalString.IndexOf(cutSymbol);

        // If the symbol is not found, IndexOf returns -1, so return an empty string
        if (symbolIndex == -1)
        {
            return string.Empty;
        }

        // We add the length of the cutSymbol to the index to start the substring AFTER the symbol
        return originalString.Substring(symbolIndex + cutSymbol.Length);
    }

    /// <summary>
    /// Разбирает дату из строки в формате "dd.MM.yyyy"
    /// </summary>
    private static DateOnly? ParseRussianDate(string? dateString)
    {
        if (string.IsNullOrWhiteSpace(dateString))
            return null;

        // Удаляем пробелы в начале и конце
        string formattedDateString = dateString.Trim();

        // Попытка 1: Формат "dd.MM.yyyy" (с опциональным временем)
        var dateOnlyMatch = Regex.Match(formattedDateString, @"(\d{1,2})\.(\d{1,2})\.(\d{4})");
        if (dateOnlyMatch.Success)
        {
            if (DateOnly.TryParseExact(dateOnlyMatch.Value, "dd.MM.yyyy", null, DateTimeStyles.None, out DateOnly dateNumeric))
                return dateNumeric;
        }

        // Попытка 2: Формат "dd МЕСЯЦ yyyy г."
        return ParseRussianTextDate(formattedDateString);
    }

    /// <summary>
    /// Разбирает дату из строки в формате "dd МЕСЯЦ yyyy г."
    /// </summary>
    /// <param name="dateString"></param>
    /// <returns></returns>
    private static DateOnly? ParseRussianTextDate(string dateString)
    {
        // Словарь русских месяцев
        var monthMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            { "января", 1 }, { "янв", 1 },
            { "февраля", 2 }, { "фев", 2 },
            { "марта", 3 }, { "мар", 3 },
            { "апреля", 4 }, { "апр", 4 },
            { "мая", 5 },
            { "июня", 6 }, { "июн", 6 },
            { "июля", 7 }, { "июл", 7 },
            { "августа", 8 }, { "авг", 8 },
            { "сентября", 9 }, { "сен", 9 },
            { "октября", 10 }, { "окт", 10 },
            { "ноября", 11 }, { "ноя", 11 },
            { "декабря", 12 }, { "дек", 12 }
        };

        // Паттерн для русских дат
        var pattern = @"(\d{1,2})\s+([а-яА-Я]+)\s+(\d{4})\s*г?\.?";
        var match = Regex.Match(dateString, pattern);

        if (match.Success)
        {
            if (int.TryParse(match.Groups[1].Value, out int day) &&
                int.TryParse(match.Groups[3].Value, out int year))
            {
                string monthName = match.Groups[2].Value;
                if (monthMap.TryGetValue(monthName, out int month))
                {
                    try
                    {
                        return new DateOnly(year, month, day);
                    }
                    catch (ArgumentOutOfRangeException)
                    {
                        return null;
                    }
                }
            }
        }

        return null;
    }

    /// <summary>
    /// Проверяет строку на соответсвие позиции комплектации
    /// Основные признаки позиции:
    ///     первый столбец - всегда целое число
    ///     второй и другие столбцы кроме последнего - текст
    ///     послежний снова - целое число
    /// </summary>
    /// <param name="row">Словарь, ключ - номер столбца, значение - текст в ячейке</param>
    /// <returns>true если позиция, "позиция", количество </returns>
    private Tuple<bool, string, int> check_position(Dictionary<int, string> row)
    {
        if (is_string_int(row[1]))
        {
            // пока количество сделаем жесткий 9 столбец
            // отсортируем по возрастанию
            List<int> column_numbers = row.Keys.OrderBy(x => x).ToList();
            string position_text = "";
            
            //int last_position = column_numbers.Max();
            // пока оставим жестко позицию 9
            int last_position = 9;
            if (!is_string_int(row[last_position]))
                return new Tuple<bool, string, int>(false, "false", 0);
            foreach (int item in column_numbers)
            {
                if (item != 1 && item !=last_position)
                { 
                    position_text += row[item];
                }
            }
            
            // считываем количество
            int amount = 0;
            if (int.TryParse(row[last_position], out int number))
                amount = number;
            else
                return new Tuple<bool, string, int>(false, "false", 0);
            
            return new Tuple<bool, string, int>(true, position_text, amount);
        }
        return new Tuple<bool, string, int>(false, "false", 0);
    }


    /// <summary>
    /// проверяет является ли строка заголовком группы деталей
    /// </summary>
    /// <param name="row">Словарь, ключ - номер столбца, значение - текст в ячейке</param>
    /// <returns>true - если заголовок, "Наименование заголовка" </returns>
    private Tuple<bool, string> check_directory(Dictionary<int, string> row)
    {
        // если поле пустое
        if (row.Count == 0)
            return new Tuple<bool, string>(false, "");
        // если значимых ячеек больше чем одна
        if (row.Count == 2)
            if (row[1] == "" && row.ContainsKey(2))
            {   //Здесь надо проверить может это общий вес или общий объем
                if (cheсk_weight(row).Item1)
                    return new Tuple<bool, string>(false, "false");
                if (cheсk_volume(row).Item1)
                    return new Tuple<bool, string>(false, "false");
                // если это не вес и не объем, то возвращаем текст
                if (!is_string_int(row[2]))
                    return new Tuple<bool, string>(true, row[2]);
            }
        // Если первый символ цифра
        if (row.Count ==1)
        {
            if (row.ContainsKey(2))
                return new Tuple<bool, string>(true, row[2]);
        }          
        if (is_string_int(row[1]))
            return new Tuple<bool, string>(false, row[1]);
        return new Tuple<bool, string>(false, "false");
    }

    /// <summary>
    /// Проверяет является ли строка целым числом
    /// </summary>
    /// <param name="str">Строка</param>
    /// <returns>true если является, false - если нет</returns>
    private bool is_string_int(string str)
    {
        if (int.TryParse(str, out int number))
            return true;
        else
            return false;
    }

    /// <summary>
    /// проверяет является ли строка весом
    /// </summary>
    /// <param name="row">Словарь, ключ - номер столбца, значение - текст в ячейке</param>
    /// <returns>кортеж true, если вес обнаружен и значение, false если нет</returns>
    private Tuple<bool, float> cheсk_weight(Dictionary<int, string> row)
    {
        if (row.Count == 0)
            return new Tuple<bool, float>(false, 0.0f);
        if (row.Count == 2)
            if (row.ContainsKey(2) && row[2].Length > 10)
            {
                string a = row[2].Substring(0, 10);
                if (row[2].Substring(0, 10) == "Общий вес:")
                {
                    // распознаем цифры
                    float? NullableFloat = ExtractNumberFromString(row[2]);
                    if (NullableFloat.HasValue)
                    {                                                        
                        return new Tuple<bool, float>(true, NullableFloat.Value);
                    }
                }                           
            }
        return new Tuple<bool, float>(false, 0.0f);
    }


    /// <summary>
    /// проверяет является ли строка весом
    /// </summary>
    /// <param name="row">Словарь, ключ - номер столбца, значение - текст в ячейке</param>
    /// <returns>кортеж true, если вес обнаружен и значение, false если нет</returns>
    private Tuple<bool, float> cheсk_volume(Dictionary<int, string> row)
    {
        if (row.Count == 0)
            return new Tuple<bool, float>(false, 0.0f);
        if (row.Count == 2)
            if (row.ContainsKey(2) && row[2].Length > 10)
            {
                string a = row[2].Substring(0, 12);
                if (row[2].Substring(0, 12) == "Общий объем:")
                {
                    // распознаем цифры
                    float? NullableFloat = ExtractNumberFromString(row[2]);
                    if (NullableFloat.HasValue)
                    {
                        return new Tuple<bool, float>(true, NullableFloat.Value);
                    }
                }
            }
        return new Tuple<bool, float>(false, 0.0f);

    }

    /// <summary>
    /// Ищет в текстовой строке число и возвращает их во float?
    /// </summary>
    /// <param name="input">Текстовая строка</param>
    /// <returns></returns>
    public static float? ExtractNumberFromString(string input)
    {
        // Regular expression pattern to match both integers and floats
        // with either '.' or ',' as the decimal separator, considering
        // the decimal separator might be followed by more digits
        var numberPattern = new Regex(@"\d+(?:[.,]\d+)?", RegexOptions.CultureInvariant);

        // Find the first occurrence of the pattern in the string
        var match = numberPattern.Match(input);

        // If a match is found, attempt to convert it to a float
        if (match.Success)
        {
            var numberString = match.Value;
            // Replace ',' with '.' if ',' was used as the decimal separator
            // to facilitate conversion to float
            if (numberString.Contains(","))
                numberString = numberString.Replace(",", ".");

            // Attempt to parse the string to a float
            CultureInfo userCulture = CultureInfo.InvariantCulture; // or get from user preferences
            if (float.TryParse(numberString, NumberStyles.Float, userCulture, out float number))
                return number;
            else
                return null;
                //throw new FormatException($"Failed to convert '{numberString}' to a float.");
        }

        // If no match is found or conversion fails, return null
        return null;
    }

    public static string CutFirstSpaces(string? input, bool removeInnerSpaces = false)
    {
        if (string.IsNullOrEmpty(input)) return string.Empty;

        // Удаляем пробелы в начале строки
        string trimmed = input.TrimStart(' ', '\t');

        // Если нужно, удаляем все пробелы внутри строки
        if (removeInnerSpaces)
        {
            trimmed = trimmed.Replace(" ", "").Replace("\t", "");
        }

        return trimmed;
    }

}