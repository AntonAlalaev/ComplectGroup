using ComplectGroup.Application.DTOs;
using ComplectGroup.Application.Interfaces;
using ComplectGroup.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ComplectGroup.Web.Controllers;

[Authorize]
public class PlanningController : Controller
{
    private readonly IBrigadeService _brigadeService;
    private readonly IOperationTypeService _operationTypeService;
    private readonly IPartOperationService _partOperationService;
    private readonly IPartService _partService;
    private readonly IChapterService _chapterService;
    private readonly ILogger<PlanningController> _logger;

    public PlanningController(
        IBrigadeService brigadeService,
        IOperationTypeService operationTypeService,
        IPartOperationService partOperationService,
        IPartService partService,
        IChapterService chapterService,
        ILogger<PlanningController> logger)
    {
        _brigadeService = brigadeService;
        _operationTypeService = operationTypeService;
        _partOperationService = partOperationService;
        _partService = partService;
        _chapterService = chapterService;
        _logger = logger;
    }

    // ===== ОБЗОР =====
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        try
        {
            var brigades = await _brigadeService.GetAllAsync(cancellationToken);
            var operationTypes = await _operationTypeService.GetAllAsync(cancellationToken);
            var summaries = await _partOperationService.GetPartSummariesAsync(null, null, cancellationToken);

            ViewBag.BrigadesCount = brigades.Count;
            ViewBag.OperationTypesCount = operationTypes.Count;
            ViewBag.PartsWithRoutesCount = summaries.Count(s => s.OperationsCount > 0);
            ViewBag.TotalOperations = summaries.Sum(s => s.OperationsCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при загрузке раздела нормирования");
        }

        return View();
    }

    // ===== БРИГАДЫ =====
    [HttpGet]
    public async Task<IActionResult> Brigades(CancellationToken cancellationToken)
    {
        try
        {
            return View(await _brigadeService.GetAllAsync(cancellationToken));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при загрузке бригад");
            TempData["Error"] = "Ошибка при загрузке бригад";
            return View(new List<BrigadeDto>());
        }
    }

    [HttpGet]
    [Authorize(Policy = "CanManageParts")]
    public IActionResult CreateBrigade()
    {
        return View("BrigadeForm", new BrigadeFormViewModel { IsActive = true });
    }

    [HttpPost]
    [Authorize(Policy = "CanManageParts")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateBrigade(BrigadeFormViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return View("BrigadeForm", model);

        try
        {
            await _brigadeService.CreateAsync(ToBrigadeRequest(model), cancellationToken);
            TempData["Success"] = "Бригада успешно создана";
            return RedirectToAction(nameof(Brigades));
        }
        catch (Exception ex) when (ex is ArgumentException || ex is InvalidOperationException || ex is KeyNotFoundException)
        {
            ModelState.AddModelError("", ex.Message);
            return View("BrigadeForm", model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при создании бригады");
            ModelState.AddModelError("", "Ошибка при сохранении бригады");
            return View("BrigadeForm", model);
        }
    }

    [HttpGet]
    [Authorize(Policy = "CanManageParts")]
    public async Task<IActionResult> EditBrigade(int id, CancellationToken cancellationToken)
    {
        try
        {
            var dto = await _brigadeService.GetByIdAsync(id, cancellationToken);
            return View("BrigadeForm", new BrigadeFormViewModel
            {
                Id = dto.Id,
                Name = dto.Name,
                Description = dto.Description,
                IsActive = dto.IsActive
            });
        }
        catch (KeyNotFoundException ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToAction(nameof(Brigades));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при загрузке бригады {Id}", id);
            TempData["Error"] = "Ошибка при загрузке бригады";
            return RedirectToAction(nameof(Brigades));
        }
    }

    [HttpPost]
    [Authorize(Policy = "CanManageParts")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditBrigade(BrigadeFormViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return View("BrigadeForm", model);

        try
        {
            await _brigadeService.UpdateAsync(model.Id, ToBrigadeRequest(model), cancellationToken);
            TempData["Success"] = "Бригада успешно обновлена";
            return RedirectToAction(nameof(Brigades));
        }
        catch (Exception ex) when (ex is ArgumentException || ex is InvalidOperationException || ex is KeyNotFoundException)
        {
            ModelState.AddModelError("", ex.Message);
            return View("BrigadeForm", model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при обновлении бригады {Id}", model.Id);
            ModelState.AddModelError("", "Ошибка при сохранении бригады");
            return View("BrigadeForm", model);
        }
    }

    [HttpPost]
    [Authorize(Policy = "CanManageParts")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteBrigade(int id, CancellationToken cancellationToken)
    {
        try
        {
            await _brigadeService.DeleteAsync(id, cancellationToken);
            TempData["Success"] = "Бригада успешно удалена";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при удалении бригады {Id}", id);
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Brigades));
    }

    // ===== ТИПЫ ОПЕРАЦИЙ =====
    [HttpGet]
    public async Task<IActionResult> OperationTypes(CancellationToken cancellationToken)
    {
        try
        {
            return View(await _operationTypeService.GetAllAsync(cancellationToken));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при загрузке типов операций");
            TempData["Error"] = "Ошибка при загрузке типов операций";
            return View(new List<OperationTypeDto>());
        }
    }

    [HttpGet]
    [Authorize(Policy = "CanManageParts")]
    public IActionResult CreateOperationType()
    {
        return View("OperationTypeForm", new OperationTypeFormViewModel());
    }

    [HttpPost]
    [Authorize(Policy = "CanManageParts")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateOperationType(OperationTypeFormViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return View("OperationTypeForm", model);

        try
        {
            await _operationTypeService.CreateAsync(ToOperationTypeRequest(model), cancellationToken);
            TempData["Success"] = "Тип операции успешно создан";
            return RedirectToAction(nameof(OperationTypes));
        }
        catch (Exception ex) when (ex is ArgumentException || ex is InvalidOperationException || ex is KeyNotFoundException)
        {
            ModelState.AddModelError("", ex.Message);
            return View("OperationTypeForm", model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при создании типа операции");
            ModelState.AddModelError("", "Ошибка при сохранении типа операции");
            return View("OperationTypeForm", model);
        }
    }

    [HttpGet]
    [Authorize(Policy = "CanManageParts")]
    public async Task<IActionResult> EditOperationType(int id, CancellationToken cancellationToken)
    {
        try
        {
            var dto = await _operationTypeService.GetByIdAsync(id, cancellationToken);
            return View("OperationTypeForm", new OperationTypeFormViewModel
            {
                Id = dto.Id,
                Code = dto.Code,
                Name = dto.Name
            });
        }
        catch (KeyNotFoundException ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToAction(nameof(OperationTypes));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при загрузке типа операции {Id}", id);
            TempData["Error"] = "Ошибка при загрузке типа операции";
            return RedirectToAction(nameof(OperationTypes));
        }
    }

    [HttpPost]
    [Authorize(Policy = "CanManageParts")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditOperationType(OperationTypeFormViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return View("OperationTypeForm", model);

        try
        {
            await _operationTypeService.UpdateAsync(model.Id, ToOperationTypeRequest(model), cancellationToken);
            TempData["Success"] = "Тип операции успешно обновлён";
            return RedirectToAction(nameof(OperationTypes));
        }
        catch (Exception ex) when (ex is ArgumentException || ex is InvalidOperationException || ex is KeyNotFoundException)
        {
            ModelState.AddModelError("", ex.Message);
            return View("OperationTypeForm", model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при обновлении типа операции {Id}", model.Id);
            ModelState.AddModelError("", "Ошибка при сохранении типа операции");
            return View("OperationTypeForm", model);
        }
    }

    [HttpPost]
    [Authorize(Policy = "CanManageParts")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteOperationType(int id, CancellationToken cancellationToken)
    {
        try
        {
            await _operationTypeService.DeleteAsync(id, cancellationToken);
            TempData["Success"] = "Тип операции успешно удалён";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при удалении типа операции {Id}", id);
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(OperationTypes));
    }

    // ===== МАРШРУТЫ ДЕТАЛЕЙ =====
    [HttpGet]
    public async Task<IActionResult> PartOperations(
        int? chapterId,
        string? search,
        int pageNumber = 1,
        int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var chapters = await _chapterService.GetAllAsync(cancellationToken);
            var summaries = await _partOperationService.GetPartSummariesAsync(chapterId, search, cancellationToken);

            var allowedPageSizes = new[] { 25, 50, 100, 200 };
            if (!allowedPageSizes.Contains(pageSize))
                pageSize = 50;

            var totalItems = summaries.Count;
            var totalPages = Math.Max(1, (int)Math.Ceiling(totalItems / (double)pageSize));
            pageNumber = Math.Clamp(pageNumber, 1, totalPages);

            var pagedSummaries = summaries
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var model = new PartOperationsIndexViewModel
            {
                Parts = pagedSummaries,
                Chapters = chapters,
                ChapterId = chapterId,
                Search = search,
                TotalOperations = summaries.Sum(s => s.OperationsCount)
            };

            ViewBag.Pagination = new PaginationInfo
            {
                CurrentPage = pageNumber,
                TotalPages = totalPages,
                TotalItems = totalItems,
                PageSize = pageSize
            };
            ViewBag.PageSizes = allowedPageSizes;

            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при загрузке списка деталей для нормирования");
            TempData["Error"] = "Ошибка при загрузке данных";
            return View(new PartOperationsIndexViewModel());
        }
    }

    [HttpGet]
    public async Task<IActionResult> PartRoute(int partId, CancellationToken cancellationToken)
    {
        try
        {
            var part = await _partService.GetByIdAsync(partId, cancellationToken);
            var operations = await _partOperationService.GetByPartIdAsync(partId, cancellationToken);

            var model = new PartRouteViewModel
            {
                Part = part!,
                Operations = operations,
                TotalPlannedHours = operations.Where(o => o.IsActive).Sum(o => o.PlannedLaborHours)
            };

            return View(model);
        }
        catch (KeyNotFoundException ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToAction(nameof(PartOperations));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при загрузке маршрута детали {PartId}", partId);
            TempData["Error"] = "Ошибка при загрузке маршрута детали";
            return RedirectToAction(nameof(PartOperations));
        }
    }

    [HttpGet]
    [Authorize(Policy = "CanManageParts")]
    public async Task<IActionResult> CreatePartOperation(int partId, CancellationToken cancellationToken)
    {
        try
        {
            var part = await _partService.GetByIdAsync(partId, cancellationToken);
            var existingOperations = await _partOperationService.GetByPartIdAsync(partId, cancellationToken);

            var model = new PartOperationFormViewModel
            {
                PartId = part!.Id,
                PartName = part!.Name,
                ChapterName = part!.Chapter.Name,
                SequenceNumber = (existingOperations.Select(o => o.SequenceNumber).DefaultIfEmpty(0).Max()) + 1,
                PlannedLaborHours = 1,
                IsActive = true
            };

            await FillPartOperationSelectListsAsync(model, cancellationToken);
            return View("PartOperationForm", model);
        }
        catch (KeyNotFoundException ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToAction(nameof(PartOperations));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при подготовке формы операции для детали {PartId}", partId);
            TempData["Error"] = "Ошибка при подготовке формы";
            return RedirectToAction(nameof(PartOperations));
        }
    }

    [HttpPost]
    [Authorize(Policy = "CanManageParts")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreatePartOperation(
        PartOperationFormViewModel model,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            await FillPartOperationSelectListsAsync(model, cancellationToken);
            return View("PartOperationForm", model);
        }

        try
        {
            await _partOperationService.CreateAsync(ToPartOperationRequest(model), cancellationToken);
            TempData["Success"] = "Операция добавлена в маршрут детали";
            return RedirectToAction(nameof(PartRoute), new { partId = model.PartId });
        }
        catch (Exception ex) when (ex is ArgumentException || ex is InvalidOperationException || ex is KeyNotFoundException)
        {
            ModelState.AddModelError("", ex.Message);
            await FillPartOperationSelectListsAsync(model, cancellationToken);
            return View("PartOperationForm", model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при добавлении операции к детали {PartId}", model.PartId);
            ModelState.AddModelError("", "Ошибка при сохранении операции");
            await FillPartOperationSelectListsAsync(model, cancellationToken);
            return View("PartOperationForm", model);
        }
    }

    [HttpGet]
    [Authorize(Policy = "CanManageParts")]
    public async Task<IActionResult> EditPartOperation(int id, CancellationToken cancellationToken)
    {
        try
        {
            var operation = await _partOperationService.GetByIdAsync(id, cancellationToken);

            var model = new PartOperationFormViewModel
            {
                Id = operation.Id,
                PartId = operation.PartId,
                PartName = operation.PartName,
                ChapterName = operation.ChapterName,
                SequenceNumber = operation.SequenceNumber,
                OperationTypeId = operation.OperationTypeId,
                BrigadeId = operation.BrigadeId,
                PlannedLaborHours = operation.PlannedLaborHours,
                Notes = operation.Notes,
                IsActive = operation.IsActive
            };

            await FillPartOperationSelectListsAsync(model, cancellationToken);
            return View("PartOperationForm", model);
        }
        catch (KeyNotFoundException ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToAction(nameof(PartOperations));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при загрузке операции {Id}", id);
            TempData["Error"] = "Ошибка при загрузке операции";
            return RedirectToAction(nameof(PartOperations));
        }
    }

    [HttpPost]
    [Authorize(Policy = "CanManageParts")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditPartOperation(
        PartOperationFormViewModel model,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            await FillPartOperationSelectListsAsync(model, cancellationToken);
            return View("PartOperationForm", model);
        }

        try
        {
            await _partOperationService.UpdateAsync(model.Id, ToPartOperationRequest(model), cancellationToken);
            TempData["Success"] = "Операция обновлена";
            return RedirectToAction(nameof(PartRoute), new { partId = model.PartId });
        }
        catch (Exception ex) when (ex is ArgumentException || ex is InvalidOperationException || ex is KeyNotFoundException)
        {
            ModelState.AddModelError("", ex.Message);
            await FillPartOperationSelectListsAsync(model, cancellationToken);
            return View("PartOperationForm", model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при обновлении операции {Id}", model.Id);
            ModelState.AddModelError("", "Ошибка при сохранении операции");
            await FillPartOperationSelectListsAsync(model, cancellationToken);
            return View("PartOperationForm", model);
        }
    }

    [HttpPost]
    [Authorize(Policy = "CanManageParts")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeletePartOperation(int id, CancellationToken cancellationToken)
    {
        int? partId = null;

        try
        {
            var operation = await _partOperationService.GetByIdAsync(id, cancellationToken);
            partId = operation.PartId;

            await _partOperationService.DeleteAsync(id, cancellationToken);
            TempData["Success"] = "Операция удалена";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при удалении операции {Id}", id);
            TempData["Error"] = ex.Message;
        }

        return partId.HasValue
            ? RedirectToAction(nameof(PartRoute), new { partId })
            : RedirectToAction(nameof(PartOperations));
    }

    // ===== ВСПОМОГАТЕЛЬНОЕ =====
    private async Task FillPartOperationSelectListsAsync(
        PartOperationFormViewModel model,
        CancellationToken cancellationToken)
    {
        var brigades = await _brigadeService.GetAllAsync(cancellationToken);
        var operationTypes = await _operationTypeService.GetAllAsync(cancellationToken);

        model.Brigades = brigades
            .OrderBy(b => b.Name)
            .Select(b => new SelectListItem
            {
                Value = b.Id.ToString(),
                Text = b.IsActive ? b.Name : $"{b.Name} (неактивна)"
            })
            .ToList();

        model.OperationTypes = operationTypes
            .OrderBy(o => o.Name)
            .Select(o => new SelectListItem
            {
                Value = o.Id.ToString(),
                Text = $"{o.Code} — {o.Name}"
            })
            .ToList();
    }

    private static BrigadeRequest ToBrigadeRequest(BrigadeFormViewModel model) => new()
    {
        Name = model.Name,
        Description = model.Description,
        IsActive = model.IsActive
    };

    private static OperationTypeRequest ToOperationTypeRequest(OperationTypeFormViewModel model) => new()
    {
        Code = model.Code,
        Name = model.Name
    };

    private static PartOperationRequest ToPartOperationRequest(PartOperationFormViewModel model) => new()
    {
        PartId = model.PartId,
        BrigadeId = model.BrigadeId,
        OperationTypeId = model.OperationTypeId,
        SequenceNumber = model.SequenceNumber,
        PlannedLaborHours = model.PlannedLaborHours,
        Notes = model.Notes,
        IsActive = model.IsActive
    };
}
