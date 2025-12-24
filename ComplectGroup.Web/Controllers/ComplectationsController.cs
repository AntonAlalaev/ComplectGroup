using ComplectGroup.Application.DTOs;
using ComplectGroup.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ComplectGroup.Web.Controllers;
using ComplectGroup.Web.Models;

namespace ComplectGroup.Web.Controllers;

public class ComplectationsController : Controller
{
    private readonly IComplectationService _complectationService;
    private readonly IComplectationImportService _importService;
    private readonly IWarehouseService _warehouseService;
    private readonly ILogger<ComplectationsController> _logger;

    public ComplectationsController(
        IComplectationService complectationService,
        IComplectationImportService importService,
        IWarehouseService warehouseService,
        ILogger<ComplectationsController> logger)
    {
        _complectationService = complectationService;
        _importService = importService;
        _warehouseService = warehouseService;
        _logger = logger;
    }

    // GET: /Complectations
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        try
        {
            var dtos = await _complectationService.GetAllAsync(cancellationToken);
            return View(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при загрузке списка комплектаций");
            TempData["Error"] = "Не удалось загрузить список комплектаций.";
            return View(new List<ComplectationDto>());
        }
    }

    // GET: /Complectations/Details/5
    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
    {
        try
        {
            var dto = await _complectationService.GetByIdAsync(id, cancellationToken);
            return View(dto);
        }
        catch (KeyNotFoundException)
        {
            TempData["Error"] = "Комплектация не найдена.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при загрузке комплектации ID={Id}", id);
            TempData["Error"] = "Произошла ошибка при загрузке.";
            return RedirectToAction(nameof(Index));
        }
    }

    // GET: /Complectations/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: /Complectations/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        CreateComplectationRequest request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(request);
        }

        try
        {
            var dto = await _complectationService.CreateAsync(request, cancellationToken);
            TempData["Success"] = "Комплектация успешно создана.";
            return RedirectToAction(nameof(Details), new { id = dto.Id });
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError("", ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при создании комплектации");
            ModelState.AddModelError("", "Произошла ошибка при сохранении.");
        }

        return View(request);
    }

    // GET: /Complectations/Edit/5
    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        try
        {
            var dto = await _complectationService.GetByIdAsync(id, cancellationToken);

            var request = new UpdateComplectationRequest
            {
                Number = dto.Number,
                Manager = dto.Manager,
                Address = dto.Address,
                Customer = dto.Customer,
                ShippingDate = dto.ShippingDate,
                ShippingTerms = dto.ShippingTerms,
                TotalWeight = dto.TotalWeight,
                TotalVolume = dto.TotalVolume,
                Positions = dto.Positions.Select(p => new UpdatePositionRequest
                {
                    Id = p.Id,
                    PartId = p.Part.Id,
                    Quantity = p.Quantity,
                    IsDeleted = false,
                    PartName = p.Part.Name,
                    PartChapterName = p.Part.Chapter.Name
                }).ToList()
            };

            return View(request);
        }
        catch (KeyNotFoundException)
        {
            TempData["Error"] = "Комплектация не найдена.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при загрузке комплектации для редактирования ID={Id}", id);
            TempData["Error"] = "не удалось загрузить данные для редактирования.";
            return RedirectToAction(nameof(Index));
        }
    }


    // POST: /Complectations/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int id,
        UpdateComplectationRequest request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(request);
        }

        try
        {
            await _complectationService.UpdateAsync(id, request, cancellationToken);
            TempData["Success"] = "Комплектация успешно обновлена.";
            return RedirectToAction(nameof(Details), new { id = id });
        }
        catch (KeyNotFoundException)
        {
            TempData["Error"] = "Комплектация не найдена.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при обновлении комплектации ID={Id}", id);
            ModelState.AddModelError("", "Произошла ошибка при сохранении.");
        }

        return View(request);
    }

    // POST: /Complectations/Delete/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        try
        {
            await _complectationService.DeleteAsync(id, cancellationToken);
            TempData["Success"] = "Комплектация удалена.";
        }
        catch (KeyNotFoundException)
        {
            TempData["Error"] = "Комплектация уже была удалена.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при удалении комплектации ID={Id}", id);
            TempData["Error"] = "не удалось удалить комплектацию.";
        }

        return RedirectToAction(nameof(Index));
    }

    // GET: /Complectations/Import
    [HttpGet]
    public IActionResult Import()
    {
        return View();        
    }

    // POST: /Complectations/Import
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Import(IFormFile file, CancellationToken cancellationToken)
    {
        if (file == null || file.Length == 0)
        {
            ModelState.AddModelError("", "Пожалуйста, выберите файл Excel (.xlsx)");
            return View();
        }

        if (!file.FileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
        {
            ModelState.AddModelError("", "Поддерживаются только файлы формата .xlsx");
            return View();
        }

        try
        {
            using var stream = new MemoryStream();
            await file.CopyToAsync(stream, cancellationToken);
            stream.Position = 0;

            // 1. Импортируем из Excel в DTO одной комплектации
            var imported = await _importService.ImportFromExcelAsync(stream, cancellationToken);

            // 2. Преобразуем в CreateComplectationRequest
            var request = new CreateComplectationRequest
            {
                Number = imported.Number,
                Manager = imported.Manager,
                Address = imported.Address,
                Customer = imported.Customer,
                ShippingDate = imported.ShippingDate,
                CreatedDate = imported.CreatedDate,
                ShippingTerms = imported.ShippingTerms,
                TotalWeight = imported.TotalWeight,
                TotalVolume = imported.TotalVolume,
                Positions = imported.Positions
                    .Select(p => new CreatePositionRequest
                    {
                        PartId = p.Part.Id,
                        Quantity = p.Quantity
                    })
                    .ToList()
            };

            // 3. Сохраняем в БД как одну комплектацию
            var created = await _complectationService.CreateAsync(request, cancellationToken);

            TempData["Success"] = $"Комплектация {created.Number} успешно импортирована (ID = {created.Id}).";
            return RedirectToAction(nameof(Details), new { id = created.Id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при импорте комплектации из Excel");
            ModelState.AddModelError("", $"Ошибка импорта: {ex.Message}");
            return View();
        }
    }

    // GET: Complectations/Browse
    [HttpGet]
    [ResponseCache(NoStore = true, Duration = 0)]  // ← Добавить эту строку отключаем кэширование
    public async Task<IActionResult> Browse(
            [FromQuery] int? id = null, // ID комплектации для отображения
            [FromQuery] bool showOnlyDeficit = false, // показывать только дефицитные позиции
            CancellationToken cancellationToken = default)
    {
        try
        {            
            var all = await _complectationService.GetAllAsync(cancellationToken);

            if (!all.Any())
            {
                _logger.LogWarning("No complectations found");
                TempData["Error"] = "Нет комплектаций в системе";
                return View(new ComplectationBrowseViewModel { Complectations = all, SelectedComplectation = null });
            }

            var selectedId = id ?? all.First().Id;
    
            var selected = await _complectationService.GetByIdAsync(selectedId, cancellationToken);

            //var selected = await _complectationService.GetByIdAsync(selectedId, cancellationToken);
            _logger.LogInformation($"Selected ID: {selectedId}, input id was: {id}");
            if (selected == null)
            {
                _logger.LogWarning($"Complectation with id {selectedId} not found");
                TempData["Error"] = $"Комплектация с ID {selectedId} не найдена";
                return View(new ComplectationBrowseViewModel { Complectations = all, SelectedComplectation = null });
            }

            var warehouseItems = await _warehouseService.GetAllWarehouseItemsAsync(cancellationToken);
            var warehouseDict = warehouseItems.ToDictionary(w => w.Part.Id, w => w.AvailableQuantity);

            var shippings = await _warehouseService.GetAllShippingsAsync(cancellationToken);

            var positionDetails = selected.Positions
                .Select(p =>
                {
                    var warehouseQty = warehouseDict.TryGetValue(p.Part.Id, out var wh) ? wh : 0;
                    var shippedQty = shippings
                        .Where(s => s.PositionId == p.Id)
                        .Sum(s => s.Quantity);

                    return new PositionDetailRow
                    {
                        PositionId = p.Id,
                        PartId = p.Part.Id,
                        Chapter = p.Part.Chapter.Name,
                        PartName = p.Part.Name,
                        RequiredQuantity = p.Quantity,
                        WarehouseQuantity = warehouseQty,
                        ShippedQuantity = shippedQty
                    };
                })
                .OrderBy(x => x.Chapter)
                .ThenBy(x => x.PartName)
                .ToList();

            // === НОВОЕ: Фильтруем по дефициту если нужно ===
            if (showOnlyDeficit)
            {
                positionDetails = positionDetails.Where(x => x.Deficit > 0).ToList();
            }

            var vm = new ComplectationBrowseViewModel
            {
                Complectations = all,
                SelectedComplectation = selected,
                PositionDetails = positionDetails,
                ShowOnlyDeficit = showOnlyDeficit
            };
            _logger.LogInformation($"Returning view with: selectedId={selected?.Id}, showOnlyDeficit={showOnlyDeficit}");
            return View(vm);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка загрузки Browse");
            TempData["Error"] = "Ошибка при загрузке данных";
            return RedirectToAction(nameof(Index));
        }
    }


    // POST: delete position from complectation /Complectations/Details/DeletePosition
    [HttpPost]
    [ValidateAntiForgeryToken]
    /// <summary>
    /// Удаляет позицию из комплектации по ID позиции 
    /// </summary>
    /// <param name="id">ID комплектации</param>
    /// <param name="positionId">ID позиции</param>
    public async Task<IActionResult> DeletePosition(int id, int positionId, CancellationToken cancellationToken)
    {
        try
        {
            // Загружаем текущую комплектацию
            var dto = await _complectationService.GetByIdAsync(id, cancellationToken);

            // Собираем запрос на обновление с пометкой позиции как удалённой
            var updateRequest = new UpdateComplectationRequest
            {
                // основные поля можно не менять
                Positions = dto.Positions.Select(p => new UpdatePositionRequest
                {
                    Id = p.Id,
                    PartId = p.Part.Id,
                    Quantity = p.Quantity,
                    IsDeleted = (p.Id == positionId)   // помечаем нужную позицию как удалённую
                }).ToList()
            };

            await _complectationService.UpdateAsync(id, updateRequest, cancellationToken);

            TempData["Success"] = "Позиция удалена.";
        }
        catch (KeyNotFoundException)
        {
            TempData["Error"] = "Комплектация или позиция не найдены.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при удалении позиции {PositionId} из комплектации ID={Id}", positionId, id);
            TempData["Error"] = "Не удалось удалить позицию.";
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    // GET: /Complectations/ReportByDates
    [HttpGet]
    public async Task<IActionResult> ReportByDates(
        DateOnly? from,
        DateOnly? to,
        string? chapter,
        bool onlyDeficit = false, // добавили параметр для фильтрации по дефициту
        CancellationToken cancellationToken = default)
    {
        //var all = await _complectationService.GetAllAsync(cancellationToken);
        // Вместо GetAllAsync используем GetNotFullyShippedAsync, только те комплектации, которые не полностью отгружены
        var all = await _complectationService.GetNotFullyShippedAsync(cancellationToken);

        var filtered = all.AsEnumerable();
        if (from.HasValue)
            filtered = filtered.Where(c => c.ShippingDate >= from.Value);
        if (to.HasValue)
            filtered = filtered.Where(c => c.ShippingDate <= to.Value);

        var list = filtered.ToList();
        // ДОБАВИЛИ: Получаем все товары на складе
        var warehouseItems = await _warehouseService.GetAllWarehouseItemsAsync(cancellationToken);
        var warehouseDict = warehouseItems.ToDictionary(w => w.Part.Id, w => w.AvailableQuantity);

        // NEW: все отгрузки
        var shippings = await _warehouseService.GetAllShippingsAsync(cancellationToken);

        var model = BuildReportViewModel(list, from, to, chapter, warehouseDict, shippings, onlyDeficit);
        return View(model);
    }

    /// <summary>
    /// Строит модель для отчета по комплектациям
    /// </summary>
    /// <param name="complectations"></param>
    /// <param name="from"></param>
    /// <param name="to"></param>
    /// <param name="chapterFilter"></param>
    /// <param name="warehouseDict"></param>
    /// <returns></returns>
    private PartsReportViewModel BuildReportViewModel(
        List<ComplectationDto> complectations,
        DateOnly? from,
        DateOnly? to,
        string? chapterFilter,
        Dictionary<int, int> warehouseDict,
        List<ShippingTransactionDto> shippings,
        bool onlyDeficit = false)
    {
        // все уникальные разделы (для дропдауна)
        var allChapters = complectations
            .SelectMany(c => c.Positions.Select(p => p.Part.Chapter.Name))
            .Distinct()
            .OrderBy(x => x)
            .ToList();

        // применяем фильтр по разделу, если задан и не "*"
        var filteredComplectations = complectations;

        var rows = new List<PartsReportRow>();

        var grouped = filteredComplectations
            .SelectMany(c => c.Positions.Select(p => new { c, p }))
            .GroupBy(x => new
            {
                Chapter = x.p.Part.Chapter.Name,
                PartName = x.p.Part.Name,
                PartId = x.p.Part.Id  // ДОБАВИЛИ PartId
            });

        if (!string.IsNullOrWhiteSpace(chapterFilter) && chapterFilter != "*")
        {
            grouped = grouped.Where(g => g.Key.Chapter == chapterFilter);
        }

        // Заголовки колонок по комплектациям
        var complectationColumns = filteredComplectations
            .OrderBy(c => c.ShippingDate)
            .ThenBy(c => c.Number)
            .Select(c => new PartsReportComplectationColumn
            {
                ComplectationId = c.Id,
                ShippingDate = c.ShippingDate,
                Number = c.Number,
                TotalWeight = c.TotalWeight,
                TotalVolume = c.TotalVolume
            })
            .ToList();

        foreach (var g in grouped.OrderBy(g => g.Key.Chapter).ThenBy(g => g.Key.PartName))
        {
            var row = new PartsReportRow
            {
                Chapter = g.Key.Chapter,
                PartName = g.Key.PartName,
                WarehouseQuantity = warehouseDict.TryGetValue(g.Key.PartId, out var whQty)
                 ? whQty 
                 : 0,
                QuantitiesByComplectation = complectationColumns.ToDictionary(
                    col => col.ComplectationId, 
                    col => 0),
                ShippedByComplectation = complectationColumns.ToDictionary(
                    col => col.ComplectationId, 
                    col => 0)    
            };

            foreach (var item in g)
            {
                var cid = item.c.Id;
                var requiredQty = item.p.Quantity;
                row.QuantitiesByComplectation[cid] += requiredQty;

                var shippedForPosition = shippings
                    .Where(s => s.PositionId == item.p.Id)
                    .Sum(s => s.Quantity);

                row.ShippedByComplectation[cid] += shippedForPosition;
            }

             // пересчитываем Итого как сумму остатка по всем комплектациям
            row.TotalQuantity = row.QuantitiesByComplectation
                .Sum(kv =>
                {
                    var cid = kv.Key;
                    var required = kv.Value;
                    var shipped = row.ShippedByComplectation.TryGetValue(cid, out var sh) ? sh : 0;
                    return Math.Max(0, required - shipped);
                });

            rows.Add(row);
        }
            // ПРИМЕНЯЕМ ФИЛЬТР ПО ДЕФИЦИТУ ЕСЛИ НУЖНО
        if (onlyDeficit)
        {
            rows = rows.Where(r => r.DeficitQuantity > 0).ToList();
        }

        return new PartsReportViewModel
        {
            From = from,
            To = to,
            SelectedChapter = string.IsNullOrWhiteSpace(chapterFilter) ? "*" : chapterFilter,
            AvailableChapters = allChapters,
            ComplectationColumns = complectationColumns,
            Rows = rows,
            OnlyDeficit = onlyDeficit // Сохраняем состояние фильтра
        };
    }






}
