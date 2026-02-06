
using Microsoft.AspNetCore.Authorization;
using ComplectGroup.Application.DTOs;
using ComplectGroup.Application.Interfaces;
using ComplectGroup.Web.Models;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ComplectGroup.Web.Controllers;


[Authorize] // ← Базовая авторизация для всего контроллера
public class WarehouseController : Controller
{
    // Репозиторий комплектации
    private readonly IComplectationRepository _complectationRepo;
    // Сервис склада
    private readonly IWarehouseService _warehouseService;
    // Работа с деталями
    private readonly IPartRepository _partRepository;
    // логгер
    private readonly ILogger<WarehouseController> _logger;
    // Сервис для работы с комплектациями
    private readonly IComplectationService _complectationService;
    // Сервис для корректировок скалада и устранения пересортицы
    private readonly ICorrectionService _correctionService;


    
    public WarehouseController(
        IWarehouseService warehouseService,
        IPartRepository partRepository,
        IComplectationService complectationService,
        IComplectationRepository complectationRepository,
        ICorrectionService correctionService,
        ILogger<WarehouseController> logger)
    {
        _warehouseService = warehouseService;
        _partRepository = partRepository;
        _complectationService = complectationService;
        _complectationRepo = complectationRepository;
        _correctionService = correctionService;
        _logger = logger;
    }

    // ===== ПРОСМОТР СКЛАДА =====
    [AllowAnonymous] // ← Переопределяет [Authorize] на уровне контроллера
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        try
        {
            var warehouseItems = await _warehouseService.GetAvailableWarehouseItemsAsync(ct);
            return View(warehouseItems);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при загрузке склада");
            TempData["Error"] = "Ошибка при загрузке данных склада";
            return RedirectToAction("Index", "Home");
        }
    }

    // ===== ПРИЁМКА - ФОРМА =====
    [Authorize(Policy = "CanReceive")]
    public async Task<IActionResult> Receipt(CancellationToken ct)
    {
        var parts = await _partRepository.GetAllAsync(ct);
        var model = new ReceiptViewModel
        {
            Parts = parts
                .OrderBy(p => p.Chapter.Name)
                .ThenBy(p => p.Name)
                .ToList()
        };
        return View(model);
    }

    // ===== ПРИЁМКА - ОБРАБОТКА =====
    [HttpPost]
    [Authorize(Policy = "CanReceive")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Receipt(ReceiptViewModel model, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            model.Parts = await _partRepository.GetAllAsync(ct);
            return View(model);
        }

        try
        {
            await _warehouseService.ReceiveAsync(
                model.PartId,
                model.Quantity,
                model.Notes,
                ct);

            TempData["Success"] = $"Товар успешно принят на склад";
            _logger.LogInformation("Приёмка: PartId={PartId}, Quantity={Quantity}", 
                model.PartId, model.Quantity);
            
            return RedirectToAction(nameof(Index));
        }
        catch (KeyNotFoundException ex)
        {
            ModelState.AddModelError("PartId", ex.Message);
            model.Parts = await _partRepository.GetAllAsync(ct);
            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при приёмке товара");
            TempData["Error"] = "Ошибка при приёмке товара: " + ex.Message;
            model.Parts = await _partRepository.GetAllAsync(ct);
            return View(model);
        }
    }

    // ===== ИСТОРИЯ ПРИЁМОК =====
    [AllowAnonymous]
    public async Task<IActionResult> ReceiptHistory(int? partId, CancellationToken ct)
    {
        try
        {
            List<ReceiptTransactionDto> transactions;

            if (partId.HasValue)
            {
                transactions = await _warehouseService.GetReceiptHistoryByPartAsync(partId.Value, ct);
            }
            else
            {
                transactions = await _warehouseService.GetAllReceiptsAsync(ct);
            }

            ViewBag.SelectedPartId = partId;
            var parts = await _partRepository.GetAllAsync(ct);
            ViewBag.Parts = new SelectList(
                parts.OrderBy(p => p.Chapter.Name).ThenBy(p => p.Name),
                "Id",
                "Name",
                partId);

            return View(transactions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при загрузке истории приёмок");
            TempData["Error"] = "Ошибка при загрузке истории";
            return RedirectToAction(nameof(Index));
        }
    }

    // ===== ОТГРУЗКА - ФОРМА =====
    [Authorize(Policy = "CanShip")]
    public async Task<IActionResult> Ship(CancellationToken ct)
    {
        var model = new ShippingViewModel();
        var complectations = await _complectationRepo.GetAllAsync(ct);
        model.Complectations = complectations
            .OrderByDescending(c => c.Id)
            .ToList();
        
        return View(model);
    }

    // ===== ОТГРУЗКА - ОБРАБОТКА =====
    // POST: /Warehouse/Ship
    [HttpPost]
    [Authorize(Policy = "CanShip")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Ship(ShippingViewModel model, CancellationToken ct)
    {
        
        if (!ModelState.IsValid)
        {
            var complectations = await _complectationRepo.GetAllAsync(ct);
            model.Complectations = complectations
                .OrderByDescending(c => c.Id)
                .ToList();
            return View(model);
        }

        try
        {
            await _warehouseService.ShipAsync(
                model.PartId,
                model.Quantity,
                model.PositionId,
                model.Notes,
                ct);

            TempData["Success"] = $"✅ Отгружено {model.Quantity} шт.";
            _logger.LogInformation($"PartId: {model.PartId}, PositionId: {model.PositionId}, Quantity: {model.Quantity}");

            // === НОВОЕ: Проверяем полноту отгрузки комплектации ===
            try
            {
                if (await _complectationService.IsFullyShippedAsync(model.PositionId, ct))
                {
                    // Получаем Position, чтобы узнать ComplectationId
                    var position = (await _complectationRepo.GetAllAsync(ct))
                        .SelectMany(c => c.Positions)
                        .FirstOrDefault(p => p.Id == model.PositionId);

                    if (position != null)
                    {
                        var complectation = (await _complectationRepo.GetAllAsync(ct))
                            .FirstOrDefault(c => c.Positions.Any(p => p.Id == model.PositionId));

                        if (complectation != null)
                        {
                            await _complectationService.MarkAsFullyShippedAsync(complectation.Id, ct);
                            TempData["Success"] += $" Комплектация №{complectation.Number} полностью отгружена! ✓";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Ошибка при проверке полноты отгрузки");
                // Не прерываем основной процесс, просто логируем
            }

            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
            var complectations = await _complectationRepo.GetAllAsync(ct);
            model.Complectations = complectations
                .OrderByDescending(c => c.Id)
                .ToList();
            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при отгрузке");
            TempData["Error"] = ex.Message;
            var complectations = await _complectationRepo.GetAllAsync(ct);
            model.Complectations = complectations
                .OrderByDescending(c => c.Id)
                .ToList();
            return View(model);
        }
    }

    // === ОТГРУЗКА ПО КОМПЛЕКТАЦИИ ===
    // POST: /Warehouse/ShipByComplectation
    [HttpPost]
    [Authorize(Policy = "CanShip")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ShipByComplectation(
        ComplectationShippingViewModel model,
        CancellationToken cancellationToken)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors);
                var errorMessage = string.Join("; ", errors.Select(e => e.ErrorMessage));
                _logger.LogError($"Ошибки валидации: {errorMessage}");

                TempData["Error"] = $"Ошибка валидации: {errorMessage}";
                model.Complectations = await _complectationService.GetAllAsync(cancellationToken);
                return View(model);
            }

            if (!model.SelectedComplectationId.HasValue)
            {
                TempData["Error"] = "Не выбрана комплектация";
                model.Complectations = await _complectationService.GetAllAsync(cancellationToken);
                return View(model);
            }

            var complectation = await _complectationService.GetByIdAsync(
                model.SelectedComplectationId.Value, cancellationToken);

            if (complectation == null)
            {
                TempData["Error"] = "Комплектация не найдена";
                return RedirectToAction(nameof(ShipByComplectation));
            }

            var validLineItems = model.LineItems
                .Where(li => li.ShippingQuantity > 0 && li.PartId > 0)
                .ToList();

            if (!validLineItems.Any())
            {
                TempData["Error"] = "Не указано количество ни для одной позиции";
                model.Complectations = await _complectationService.GetAllAsync(cancellationToken);
                return View(model);
            }

            int totalShipped = 0;

            foreach (var lineItem in validLineItems)
            {
                try
                {
                    if (lineItem.ShippingQuantity > lineItem.RemainingToShip)
                    {
                        _logger.LogWarning($"Отгружено больше, чем требуется для Position {lineItem.PositionId}. " +
                                        $"Требуется: {lineItem.RemainingToShip}, отгружается: {lineItem.ShippingQuantity}");
                    }

                    // Продолжаем выполнение без пропуска
                    var notes = $"Комплектация: №{complectation.Number}, Дата: {DateTime.Now:dd.MM.yyyy HH:mm}";

                    await _warehouseService.ShipAsync(
                        lineItem.PartId,
                        lineItem.ShippingQuantity,  // ← Будет отгружено ВСЁ, что ввёл пользователь
                        lineItem.PositionId,
                        notes,
                        cancellationToken);

                    totalShipped++;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, $"Ошибка отгрузки Part {lineItem.PartId}");
                    continue;
                }
            }

            // === НОВОЕ: Проверяем полноту отгрузки после отгрузки позиций ===
            try
            {
                if (await _complectationService.IsFullyShippedAsync(model.SelectedComplectationId.Value, cancellationToken))
                {
                    await _complectationService.MarkAsFullyShippedAsync(model.SelectedComplectationId.Value, cancellationToken);
                    TempData["Success"] = $"✅ Успешно отгружено {totalShipped} позиций. " +
                                        $"Комплектация №{complectation.Number} полностью отгружена! ✓";
                }
                else
                {
                    TempData["Success"] = $"✅ Успешно отгружено {totalShipped} позиций";
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Ошибка при проверке полноты отгрузки");
                TempData["Success"] = $"✅ Успешно отгружено {totalShipped} позиций";
            }

            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при отгрузке товаров по комплектации");
            TempData["Error"] = $"❌ Ошибка: {ex.Message}";
            return RedirectToAction(nameof(ShipByComplectation));
        }
    }



    // ===== API для AJAX =====
    [HttpGet]
    [Route("warehouse/api/positions/{complectationId}")]
    public async Task<IActionResult> GetPositions(int complectationId, CancellationToken ct)
    {
        try
        {
            var complectation = await _complectationRepo.GetByIdAsync(complectationId, ct);
            if (complectation == null)
                return NotFound();

            var positions = complectation.Positions
                .Select(p => new
                {
                    id = p.Id,
                    name = $"{p.Part.Name} (кол-во: {p.Quantity})", // Без ID в начале
                    fullName = $"[{p.Id}] {p.Part.Name} (кол-во: {p.Quantity})", // Полное имя для совместимости
                    partId = p.PartId,
                    partName = p.Part.Name, // Отдельно имя детали
                    quantity = p.Quantity // Отдельно количество
                })
                .ToList();

            return Json(positions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при загрузке позиций");
            return BadRequest();
        }
    }


    // ===== ИСТОРИЯ ОТГРУЗОК =====
    [AllowAnonymous]
    public async Task<IActionResult> ShippingHistory(int? positionId, CancellationToken ct)
    {
        try
        {
            List<ShippingTransactionDto> transactions;

            if (positionId.HasValue)
            {
                transactions = await _warehouseService.GetShippingHistoryByPositionAsync(positionId.Value, ct);
            }
            else
            {
                transactions = await _warehouseService.GetAllShippingsAsync(ct);
            }

            ViewBag.SelectedPositionId = positionId;
            return View(transactions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при загрузке истории отгрузок");
            TempData["Error"] = "Ошибка при загрузке истории";
            return RedirectToAction(nameof(Index));
        }
    }

    // Вспомогательный метод
    private async Task PopulatePartsAndPositions(ShippingViewModel model, CancellationToken ct)
    {
        var parts = await _partRepository.GetAllAsync(ct);
        model.Parts = parts
            .OrderBy(p => p.Chapter.Name)
            .ThenBy(p => p.Name)
            .ToList();
    }

    // ==== ПРИЁМКА ПО КОМПЛЕКТАЦИИ =====
    // GET: /Warehouse/ReceiptByComplectation
    [HttpGet]
    [Authorize(Policy = "CanReceive")]
    public async Task<IActionResult> ReceiptByComplectation(CancellationToken cancellationToken)
    {
        var complectations = await _complectationService.GetAllAsync(cancellationToken);
        
        var model = new ComplectationReceiptViewModel
        {
            Complectations = complectations
        };

        return View(model);
    }

    // ==== ПРИЁМКА ПО КОМПЛЕКТАЦИИ =====
    // POST: /Warehouse/ReceiptByComplectation
    [HttpPost]
    [Authorize(Policy = "CanReceive")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ReceiptByComplectation(
        ComplectationReceiptViewModel model,
        CancellationToken cancellationToken)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors);
                var errorMessage = string.Join("; ", errors.Select(e => e.ErrorMessage));
                _logger.LogError($"Ошибки валидации: {errorMessage}");
                
                TempData["Error"] = $"Ошибка валидации: {errorMessage}";
                model.Complectations = await _complectationService.GetAllAsync(cancellationToken);
                return View(model);
            }

            if (!model.SelectedComplectationId.HasValue)
            {
                TempData["Error"] = "Не выбрана комплектация";
                model.Complectations = await _complectationService.GetAllAsync(cancellationToken);
                return View(model);
            }

            var complectation = await _complectationService.GetByIdAsync(
                model.SelectedComplectationId.Value, cancellationToken);

            if (complectation == null)
            {
                TempData["Error"] = "Комплектация не найдена";
                return RedirectToAction(nameof(ReceiptByComplectation));
            }

            // КЛЮЧЕВОЕ ИЗМЕНЕНИЕ: фильтруем на сервере, а не на клиенте
            var validLineItems = model.LineItems
                .Where(li => li.ReceiptQuantity > 0 && li.PartId > 0)  // ← ДОБАВЬ ПРОВЕРКУ PartId
                .ToList();

            if (!validLineItems.Any())
            {
                TempData["Error"] = "Не указано количество ни для одной позиции";
                model.Complectations = await _complectationService.GetAllAsync(cancellationToken);
                return View(model);
            }

            int totalReceived = 0;

            foreach (var lineItem in validLineItems)
            {
                try
                {
                    var notes = $"Комплектация: №{complectation.Number}, Дата: {DateTime.Now:dd.MM.yyyy HH:mm}";

                    await _warehouseService.ReceiveAsync(
                        lineItem.PartId,
                        lineItem.ReceiptQuantity,
                        notes,
                        cancellationToken);

                    totalReceived++;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, $"Ошибка приходования Part {lineItem.PartId}");
                    // Продолжаем со следующей позиции
                    continue;
                }
            }

            TempData["Success"] = $"✅ Успешно приходовано {totalReceived} позиций";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при приходовании товаров по комплектации");
            TempData["Error"] = $"❌ Ошибка: {ex.Message}";
            return RedirectToAction(nameof(ReceiptByComplectation));
        }
    }

    // ==== ОТГРУЗКА ПО КОМПЛЕКТАЦИИ =====
    // GET: /Warehouse/GetComplectationPositions (AJAX)
    [HttpGet]
    [Authorize(Policy = "CanShip")]
    [Route("/Warehouse/GetComplectationPositions/{complectationId}")]
    public async Task<IActionResult> GetComplectationPositions(int complectationId, CancellationToken cancellationToken)
    {
        try
        {
            var complectation = await _complectationService.GetByIdAsync(complectationId, cancellationToken);

            var lineItems = complectation.Positions.Select(p => new ComplectationReceiptViewModel.ReceiptLineItem
            {
                PositionId = p.Id,
                PartId = p.Part.Id,
                Chapter = p.Part.Chapter.Name,
                PartName = p.Part.Name,
                RequiredQuantity = p.Quantity,
                ReceiptQuantity = 0// По умолчанию = 0
            }).ToList();

            return Json(lineItems);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении позиций комплектации {ComplectationId}", complectationId);
            return BadRequest(new { error = "Ошибка загрузки позиций" });
        }
    }

    // ==== ОТГРУЗКА ПО КОМПЛЕКТАЦИИ =====
    // GET: /Warehouse/ShipByComplectation
    [HttpGet]
    [Authorize(Policy = "CanShip")]
    public async Task<IActionResult> ShipByComplectation(CancellationToken cancellationToken)
    {
        try
        {
            var complectations = await _complectationService.GetAllAsync(cancellationToken);
            var model = new ComplectationShippingViewModel
            {
                Complectations = complectations
            };
            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при загрузке страницы отгрузки по комплектации");
            TempData["Error"] = "Ошибка при загрузке страницы";
            return RedirectToAction(nameof(Index));
        }
    }

    // ==== ОТГРУЗКА ПО КОМПЛЕКТАЦИИ =====
    // GET: /Warehouse/GetComplectationShippingPositions (AJAX)
    [HttpGet]
    [Authorize(Policy = "CanShip")]
    [Route("/Warehouse/GetComplectationShippingPositions/{complectationId}")]
    public async Task<IActionResult> GetComplectationShippingPositions(
        int complectationId,
        CancellationToken cancellationToken)
    {
        try
        {
            var complectation = await _complectationService.GetByIdAsync(complectationId, cancellationToken);

            if (complectation == null)
                return NotFound();

            // Получаем все отгрузки и остатки на складе
            var allShippings = await _warehouseService.GetAllShippingsAsync(cancellationToken);
            var warehouseItems = await _warehouseService.GetAllWarehouseItemsAsync(cancellationToken);
            var warehouseDict = warehouseItems.ToDictionary(w => w.Part.Id, w => w.AvailableQuantity);

            var lineItems = complectation.Positions.Select(p =>
            {
                // Получаем уже отгруженное количество для этой позиции
                var alreadyShipped = allShippings
                    .Where(s => s.PositionId == p.Id)
                    .Sum(s => s.Quantity);

                // Получаем остаток на складе
                var warehouseQty = warehouseDict.TryGetValue(p.Part.Id, out var wh) ? wh : 0;

                return new ComplectationShippingViewModel.ShippingLineItem
                {
                    PositionId = p.Id,
                    PartId = p.Part.Id,
                    Chapter = p.Part.Chapter.Name,
                    PartName = p.Part.Name,
                    RequiredQuantity = p.Quantity,
                    AlreadyShipped = alreadyShipped,
                    WarehouseQuantity = warehouseQty,
                    ShippingQuantity = 0 // по умолчанию 0 вместо остатка
                    //ShippingQuantity = Math.Max(0, p.Quantity - alreadyShipped) // По умолчанию = осталось
                };
            }).ToList();

            return Json(lineItems);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении позиций комплектации {ComplectationId}", complectationId);
            return BadRequest(new { error = "Ошибка загрузки позиций" });
        }
    }

    // ===== КОРРЕКТИРОВКА ПЕРЕСОРТИЦЫ =====
    [HttpGet]
    [Authorize(Policy = "CanCorrect")]
    public async Task<IActionResult> Correction(CancellationToken ct)
    {
        try
        {
            var parts = await _partRepository.GetAllAsync(ct);
            var model = new CorrectionViewModel
            {
                Parts = new SelectList(
                    parts.OrderBy(p => p.Chapter.Name).ThenBy(p => p.Name),
                    "Id",
                    "Name")
            };
            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при загрузке страницы корректировки");
            TempData["Error"] = "Ошибка при загрузке данных";
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost]
    [Authorize(Policy = "CanCorrect")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Correction(CorrectionViewModel model, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            model.Parts = new SelectList(
                await _partRepository.GetAllAsync(ct),
                "Id",
                "Name");
            return View(model);
        }
        
        try
        {
            // Проверка: нельзя выбрать одну и ту же деталь
            if (model.OldPartId == model.NewPartId)
            {
                ModelState.AddModelError("NewPartId", "Старая и новая детали должны быть разными");
                model.Parts = new SelectList(
                    await _partRepository.GetAllAsync(ct),
                    "Id",
                    "Name");
                return View(model);
            }
            
            var correction = await _correctionService.CreateCorrectionAsync(
                model.OldPartId,
                model.NewPartId,
                model.Quantity,
                model.Notes,
                ct);
            
            TempData["Success"] = $"✅ Корректировка {correction.CorrectionNumber} успешно выполнена!";
            return RedirectToAction(nameof(CorrectionHistory));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при выполнении корректировки");
            TempData["Error"] = $"❌ Ошибка: {ex.Message}";
            model.Parts = new SelectList(
                await _partRepository.GetAllAsync(ct),
                "Id",
                "Name");
            return View(model);
        }
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> CorrectionHistory(CancellationToken ct)
    {
        try
        {
            var corrections = await _correctionService.GetCorrectionHistoryAsync(ct);
            return View(corrections);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при загрузке истории корректировок");
            TempData["Error"] = "Ошибка при загрузке истории";
            return RedirectToAction(nameof(Index));
        }
}

}
