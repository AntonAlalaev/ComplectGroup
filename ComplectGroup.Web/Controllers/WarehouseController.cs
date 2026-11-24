
using Microsoft.AspNetCore.Authorization;
using ComplectGroup.Application.DTOs;
using ComplectGroup.Application.Interfaces;
using ComplectGroup.Web.Models;
using ComplectGroup.Infrastructure.Identity;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Identity;

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
    private readonly IProductionPlanService _productionPlanService;
    // Сервис для корректировок скалада и устранения пересортицы
    private readonly ICorrectionService _correctionService;
    // Менеджер пользователей
    private readonly UserManager<ApplicationUser> _userManager;



    public WarehouseController(
        IWarehouseService warehouseService,
        IPartRepository partRepository,
        IComplectationService complectationService,
        IProductionPlanService productionPlanService,
        IComplectationRepository complectationRepository,
        ICorrectionService correctionService,
        ILogger<WarehouseController> logger,
        UserManager<ApplicationUser> userManager)
    {
        _warehouseService = warehouseService;
        _partRepository = partRepository;
        _complectationService = complectationService;
        _productionPlanService = productionPlanService;
        _complectationRepo = complectationRepository;
        _correctionService = correctionService;
        _logger = logger;
        _userManager = userManager;
    }

    // ===== ПРОСМОТР СКЛАДА =====
    [AllowAnonymous] // ← Переопределяет [Authorize] на уровне контроллера
    public async Task<IActionResult> Index(int pageNumber = 1, int pageSize = 50, CancellationToken ct = default)
    {
        try
        {
            var warehouseItems = await _warehouseService.GetAvailableWarehouseItemsAsync(ct);

            // Стабильная сортировка, чтобы при переходе по страницам порядок не менялся
            var orderedItems = warehouseItems
                .OrderBy(x => x.Part.Chapter.Name)
                .ThenBy(x => x.Part.Name)
                .ToList();

            var allowedPageSizes = new[] { 25, 50, 100, 200 };
            if (!allowedPageSizes.Contains(pageSize))
                pageSize = 50;

            var totalItems = orderedItems.Count;
            var totalPages = Math.Max(1, (int)Math.Ceiling(totalItems / (double)pageSize));
            pageNumber = Math.Clamp(pageNumber, 1, totalPages);

            var pagedItems = orderedItems
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.Pagination = new PaginationInfo
            {
                CurrentPage = pageNumber,
                TotalPages = totalPages,
                TotalItems = totalItems,
                PageSize = pageSize
            };

            return View(pagedItems);
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
                .ToList(),
            Plans = (await _productionPlanService.GetAllAsync(ct))
                .Where(p => p.StatusCode != 3) // не закрытые
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
            model.Plans = (await _productionPlanService.GetAllAsync(ct)).Where(p => p.StatusCode != 3).ToList();
            return View(model);
        }

        try
        {
            var userId = _userManager.GetUserId(User) ?? "";
            await _warehouseService.ReceiveAsync(
                model.PartId,
                model.Quantity,
                model.Notes,
                userId,
                ct,
                model.Source,
                model.ProductionPlanId);

            TempData["Success"] = $"Товар успешно принят на склад";
            _logger.LogInformation("Приёмка: PartId={PartId}, Quantity={Quantity}, UserId={UserId}",
                model.PartId, model.Quantity, userId);

            return RedirectToAction(nameof(Index));
        }
        catch (KeyNotFoundException ex)
        {
            ModelState.AddModelError("PartId", ex.Message);
            model.Parts = await _partRepository.GetAllAsync(ct);
            model.Plans = (await _productionPlanService.GetAllAsync(ct)).Where(p => p.StatusCode != 3).ToList();
            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при приёмке товара");
            TempData["Error"] = "Ошибка при приёмке товара: " + ex.Message;
            model.Parts = await _partRepository.GetAllAsync(ct);
            model.Plans = (await _productionPlanService.GetAllAsync(ct)).Where(p => p.StatusCode != 3).ToList();
            return View(model);
        }
    }

    // ===== ИСТОРИЯ ПРИЁМОК =====
    [AllowAnonymous]
    public async Task<IActionResult> ReceiptHistory(int? partId, int pageNumber = 1, CancellationToken ct = default)
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

            // Загружаем имена пользователей
            foreach (var t in transactions)
            {
                if (!string.IsNullOrEmpty(t.UserId))
                {
                    var user = await _userManager.FindByIdAsync(t.UserId);
                    t.UserName = user?.UserName ?? $"User: {t.UserId}";
                }
                else
                {
                    t.UserName = "Неизвестно";
                }
            }

            // Пагинация
            var pageSize = 20;
            var totalItems = transactions.Count;
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            var pagedTransactions = transactions
                .OrderByDescending(t => t.ReceiptDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.SelectedPartId = partId;
            ViewBag.Pagination = new PaginationInfo
            {
                CurrentPage = pageNumber,
                TotalPages = totalPages,
                TotalItems = totalItems,
                PageSize = pageSize
            };

            var parts = await _partRepository.GetAllAsync(ct);
            ViewBag.Parts = new SelectList(
                parts.OrderBy(p => p.Chapter.Name).ThenBy(p => p.Name),
                "Id",
                "Name",
                partId);

            return View(pagedTransactions);
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
            var userId = _userManager.GetUserId(User) ?? "";
            await _warehouseService.ShipAsync(
                model.PartId,
                model.Quantity,
                model.PositionId,
                model.Notes,
                userId,
                ct);

            TempData["Success"] = $"✅ Отгружено {model.Quantity} шт.";
            _logger.LogInformation($"PartId: {model.PartId}, PositionId: {model.PositionId}, Quantity: {model.Quantity}, UserId: {userId}");

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

            // Клиент отправляет только позиции с ненулевым количеством и может разбивать
            // список на страницы. Поэтому не полагаемся на индексы полей формы и
            // группируем присланные строки по PositionId.
            var requestedItems = model.LineItems
                .Where(li => li.ShippingQuantity > 0 && li.PositionId > 0)
                .GroupBy(li => li.PositionId)
                .Select(g => new
                {
                    PositionId = g.Key,
                    Quantity = g.Sum(x => x.ShippingQuantity)
                })
                .ToList();

            if (!requestedItems.Any())
            {
                TempData["Error"] = "Не указано количество ни для одной позиции";
                model.Complectations = await _complectationService.GetAllAsync(cancellationToken);
                return View(model);
            }

            // Данные о позициях, уже отгруженном количестве и остатках склада
            // берём из БД, а не из формы: так обработка не зависит от пагинации
            // и от того, какие поля прислал клиент.
            var positionsById = complectation.Positions.ToDictionary(p => p.Id);

            var allShippings = await _warehouseService.GetAllShippingsAsync(cancellationToken);
            var shippedByPosition = allShippings
                .Where(s => s.PositionId.HasValue)
                .GroupBy(s => s.PositionId!.Value)
                .ToDictionary(g => g.Key, g => g.Sum(s => s.Quantity));

            var warehouseItems = await _warehouseService.GetAllWarehouseItemsAsync(cancellationToken);
            var availableByPart = warehouseItems
                .GroupBy(w => w.Part.Id)
                .ToDictionary(g => g.Key, g => g.Sum(w => w.AvailableQuantity));

            int totalShipped = 0;
            int skipped = 0;
            var userId = _userManager.GetUserId(User) ?? "";
            var notes = $"Комплектация: №{complectation.Number}, Дата: {DateTime.Now:dd.MM.yyyy HH:mm}";

            foreach (var requested in requestedItems)
            {
                try
                {
                    if (!positionsById.TryGetValue(requested.PositionId, out var position))
                    {
                        _logger.LogWarning(
                            "Позиция {PositionId} не принадлежит комплектации №{Number}; пропущена",
                            requested.PositionId, complectation.Number);
                        skipped++;
                        continue;
                    }

                    var alreadyShipped = shippedByPosition.GetValueOrDefault(position.Id);
                    var remainingToShip = Math.Max(0, position.Quantity - alreadyShipped);
                    var quantity = requested.Quantity;

                    if (remainingToShip <= 0)
                    {
                        _logger.LogWarning(
                            "Позиция {PositionId} уже полностью отгружена (требуется {Required}, отгружено {Shipped}); пропущена",
                            position.Id, position.Quantity, alreadyShipped);
                        skipped++;
                        continue;
                    }

                    if (quantity > remainingToShip)
                    {
                        _logger.LogWarning(
                            "Запрошено больше, чем осталось отгрузить для Position {PositionId}. " +
                            "Требуется: {Required}, уже отгружено: {Shipped}, запрошено: {Requested}",
                            position.Id, position.Quantity, alreadyShipped, quantity);
                    }

                    // Не позволяем уйти в минус по складу: учитываем остатки в рамках текущего запроса
                    var available = availableByPart.GetValueOrDefault(position.Part.Id);
                    if (quantity > available)
                    {
                        _logger.LogWarning(
                            "Недостаточно товара на складе для Part {PartId}. Доступно: {Available}, запрошено: {Requested}. Количество уменьшено",
                            position.Part.Id, available, quantity);
                        quantity = available;
                    }

                    if (quantity <= 0)
                    {
                        skipped++;
                        continue;
                    }

                    await _warehouseService.ShipAsync(
                        position.Part.Id,
                        quantity,
                        position.Id,
                        notes,
                        userId,
                        cancellationToken);

                    availableByPart[position.Part.Id] = available - quantity;
                    totalShipped++;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Ошибка отгрузки Position {PositionId}", requested.PositionId);
                    skipped++;
                    continue;
                }
            }

            if (totalShipped == 0)
            {
                TempData["Error"] = skipped > 0
                    ? $"❌ Не удалось отгрузить ни одной позиции: пропущено {skipped}. Проверьте остатки на складе."
                    : "❌ Не удалось отгрузить ни одной позиции.";
                return RedirectToAction(nameof(ShipByComplectation));
            }

            var skippedSuffix = skipped > 0 ? $", пропущено {skipped}" : "";

            // === НОВОЕ: Проверяем полноту отгрузки после отгрузки позиций ===
            try
            {
                if (await _complectationService.IsFullyShippedAsync(model.SelectedComplectationId.Value, cancellationToken))
                {
                    await _complectationService.MarkAsFullyShippedAsync(model.SelectedComplectationId.Value, cancellationToken);
                    TempData["Success"] = $"✅ Успешно отгружено {totalShipped} позиций{skippedSuffix}. " +
                                        $"Комплектация №{complectation.Number} полностью отгружена! ✓";
                }
                else
                {
                    TempData["Success"] = $"✅ Успешно отгружено {totalShipped} позиций{skippedSuffix}";
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Ошибка при проверке полноты отгрузки");
                TempData["Success"] = $"✅ Успешно отгружено {totalShipped} позиций{skippedSuffix}";
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
    public async Task<IActionResult> ShippingHistory(int? positionId, int pageNumber = 1, CancellationToken ct = default)
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

            // Загружаем имена пользователей
            foreach (var t in transactions)
            {
                if (!string.IsNullOrEmpty(t.UserId))
                {
                    var user = await _userManager.FindByIdAsync(t.UserId);
                    t.UserName = user?.UserName ?? $"User: {t.UserId}";
                }
                else
                {
                    t.UserName = "Неизвестно";
                }
            }

            // Пагинация
            var pageSize = 20;
            var totalItems = transactions.Count;
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            var pagedTransactions = transactions
                .OrderByDescending(t => t.ShippingDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.SelectedPositionId = positionId;
            ViewBag.Pagination = new PaginationInfo
            {
                CurrentPage = pageNumber,
                TotalPages = totalPages,
                TotalItems = totalItems,
                PageSize = pageSize
            };

            return View(pagedTransactions);
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

            // Клиент отправляет только позиции с ненулевым количеством и может разбивать
            // список на страницы. Группируем строки по PositionId, чтобы не зависеть
            // от индексов полей формы.
            var requestedItems = model.LineItems
                .Where(li => li.ReceiptQuantity > 0 && li.PositionId > 0)
                .GroupBy(li => li.PositionId)
                .Select(g => new
                {
                    PositionId = g.Key,
                    Quantity = g.Sum(x => x.ReceiptQuantity)
                })
                .ToList();

            if (!requestedItems.Any())
            {
                TempData["Error"] = "Не указано количество ни для одной позиции";
                model.Complectations = await _complectationService.GetAllAsync(cancellationToken);
                return View(model);
            }

            // Позиции и детали берём из БД, а не из формы.
            var positionsById = complectation.Positions.ToDictionary(p => p.Id);

            int totalReceived = 0;
            int skipped = 0;
            var userId = _userManager.GetUserId(User) ?? "";
            var notes = $"Комплектация: №{complectation.Number}, Дата: {DateTime.Now:dd.MM.yyyy HH:mm}";

            foreach (var requested in requestedItems)
            {
                try
                {
                    if (!positionsById.TryGetValue(requested.PositionId, out var position))
                    {
                        _logger.LogWarning(
                            "Позиция {PositionId} не принадлежит комплектации №{Number}; пропущена",
                            requested.PositionId, complectation.Number);
                        skipped++;
                        continue;
                    }

                    await _warehouseService.ReceiveAsync(
                        position.Part.Id,
                        requested.Quantity,
                        notes,
                        userId,
                        cancellationToken);

                    totalReceived++;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Ошибка приходования Position {PositionId}", requested.PositionId);
                    // Продолжаем со следующей позиции
                    skipped++;
                    continue;
                }
            }

            if (totalReceived == 0)
            {
                TempData["Error"] = skipped > 0
                    ? $"❌ Не удалось приходовать ни одной позиции: пропущено {skipped}."
                    : "❌ Не удалось приходовать ни одной позиции.";
                return RedirectToAction(nameof(ReceiptByComplectation));
            }

            var skippedSuffix = skipped > 0 ? $", пропущено {skipped}" : "";
            TempData["Success"] = $"✅ Успешно приходовано {totalReceived} позиций{skippedSuffix}";
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

            // Получаем все отгрузки и остатки на складе для информационных колонок
            var allShippings = await _warehouseService.GetAllShippingsAsync(cancellationToken);
            var warehouseItems = await _warehouseService.GetAllWarehouseItemsAsync(cancellationToken);
            var warehouseDict = warehouseItems.ToDictionary(w => w.Part.Id, w => w.AvailableQuantity);

            var lineItems = complectation.Positions.Select(p => new ComplectationReceiptViewModel.ReceiptLineItem
            {
                PositionId = p.Id,
                PartId = p.Part.Id,
                Chapter = p.Part.Chapter.Name,
                PartName = p.Part.Name,
                RequiredQuantity = p.Quantity,
                AlreadyShipped = allShippings
                    .Where(s => s.PositionId == p.Id)
                    .Sum(s => s.Quantity),
                WarehouseQuantity = warehouseDict.TryGetValue(p.Part.Id, out var wh) ? wh : 0,
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

            var userId = _userManager.GetUserId(User) ?? "";
            var correction = await _correctionService.CreateCorrectionAsync(
                model.OldPartId,
                model.NewPartId,
                model.Quantity,
                model.Notes,
                userId,
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
    public async Task<IActionResult> CorrectionHistory(int pageNumber = 1, CancellationToken ct = default)
    {
        try
        {
            var corrections = await _correctionService.GetCorrectionHistoryAsync(ct);

            // Загружаем имена пользователей для старых записей
            foreach (var c in corrections)
            {
                // Если CreatedBy содержит UserId (GUID), загружаем имя пользователя
                if (!string.IsNullOrEmpty(c.CreatedBy) && Guid.TryParse(c.CreatedBy, out _))
                {
                    var user = await _userManager.FindByIdAsync(c.CreatedBy);
                    c.CreatedBy = user?.UserName ?? $"User: {c.CreatedBy}";
                }
            }

            // Пагинация
            var pageSize = 20;
            var totalItems = corrections.Count;
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            var pagedCorrections = corrections
                .OrderByDescending(c => c.CorrectionDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.Pagination = new PaginationInfo
            {
                CurrentPage = pageNumber,
                TotalPages = totalPages,
                TotalItems = totalItems,
                PageSize = pageSize
            };

            return View(pagedCorrections);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при загрузке истории корректировок");
            TempData["Error"] = "Ошибка при загрузке истории";
            return RedirectToAction(nameof(Index));
        }
    }
}
