using ComplectGroup.Application.DTOs;
using ComplectGroup.Application.Interfaces;
using ComplectGroup.Web.ModelBinders;
using ComplectGroup.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ComplectGroup.Web.Controllers;

[Authorize]
public class ProductionPlansController : Controller
{
    private readonly IProductionPlanService _planService;
    private readonly IPartOperationService _partOperationService;
    private readonly IPartService _partService;
    private readonly IChapterService _chapterService;
    private readonly ILogger<ProductionPlansController> _logger;

    public ProductionPlansController(
        IProductionPlanService planService,
        IPartOperationService partOperationService,
        IPartService partService,
        IChapterService chapterService,
        ILogger<ProductionPlansController> logger)
    {
        _planService = planService;
        _partOperationService = partOperationService;
        _partService = partService;
        _chapterService = chapterService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        try
        {
            return View(await _planService.GetAllAsync(cancellationToken));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при загрузке производственных планов");
            TempData["Error"] = "Ошибка при загрузке планов";
            return View(new List<ProductionPlanDto>());
        }
    }

    [HttpGet]
    public IActionResult Create()
    {
        var today = DateTime.Today;
        var start = today.AddDays(-(int)today.DayOfWeek + (int)DayOfWeek.Monday);
        if (start > today) start = start.AddDays(-7);
        var end = start.AddDays(6);

        ViewBag.PeriodStart = start.ToString("yyyy-MM-dd");
        ViewBag.PeriodEnd = end.ToString("yyyy-MM-dd");
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        DateTime periodStart,
        DateTime periodEnd,
        string? notes,
        bool allowOverlap,
        CancellationToken cancellationToken)
    {
        try
        {
            var plan = await _planService.CreateAsync(periodStart, periodEnd, notes, allowOverlap, cancellationToken);
            TempData["Success"] = "Производственный план создан";
            return RedirectToAction(nameof(Details), new { id = plan.Id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при создании плана");
            TempData["Error"] = ex.Message;
            ViewBag.PeriodStart = periodStart.ToString("yyyy-MM-dd");
            ViewBag.PeriodEnd = periodEnd.ToString("yyyy-MM-dd");
            ViewBag.Notes = notes;
            ViewBag.AllowOverlap = allowOverlap;
            return View();
        }
    }

    [HttpGet]
    public async Task<IActionResult> Details(
        int id,
        int? chapterId,
        string? search,
        CancellationToken cancellationToken)
    {
        try
        {
            var plan = await _planService.GetByIdAsync(id, cancellationToken);
            ViewBag.Parts = await _partService.GetAllAsync(cancellationToken);
            ViewBag.PartSummaries = await _partOperationService.GetPartSummariesAsync(chapterId, search, cancellationToken);
            ViewBag.Chapters = await _chapterService.GetAllAsync(cancellationToken);
            ViewBag.ChapterId = chapterId;
            ViewBag.Search = search;
            return View(plan);
        }
        catch (KeyNotFoundException ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при загрузке плана {Id}", id);
            TempData["Error"] = "Ошибка при загрузке плана";
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddItem(
        int planId,
        int partId,
        int quantity,
        int? sourceRequirementId,
        CancellationToken cancellationToken)
    {
        try
        {
            await _planService.AddItemAsync(planId, partId, quantity, sourceRequirementId, cancellationToken);
            TempData["Success"] = "Позиция добавлена в план";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при добавлении позиции в план {PlanId}", planId);
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Details), new { id = planId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveItems(
        int planId,
        List<int> selectedPartIds,
        List<int> selectedQuantities,
        CancellationToken cancellationToken)
    {
        try
        {
            await _planService.ReplaceItemsAsync(planId, selectedPartIds ?? new List<int>(), selectedQuantities ?? new List<int>(), cancellationToken);
            TempData["Success"] = "Позиции плана сохранены";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при сохранении позиций плана {PlanId}", planId);
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Details), new { id = planId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveItem(int planId, int itemId, CancellationToken cancellationToken)
    {
        try
        {
            await _planService.RemoveItemAsync(planId, itemId, cancellationToken);
            TempData["Success"] = "Позиция удалена";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при удалении позиции {ItemId} из плана {PlanId}", itemId, planId);
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Details), new { id = planId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Approve(int planId, CancellationToken cancellationToken)
    {
        try
        {
            await _planService.ApproveAsync(planId, cancellationToken);
            TempData["Success"] = "План утверждён";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при утверждении плана {PlanId}", planId);
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Details), new { id = planId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Start(int planId, CancellationToken cancellationToken)
    {
        try
        {
            await _planService.StartAsync(planId, cancellationToken);
            TempData["Success"] = "План переведён в работу";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при переводе плана {PlanId} в работу", planId);
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Details), new { id = planId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Close(int planId, CancellationToken cancellationToken)
    {
        try
        {
            await _planService.CloseAsync(planId, cancellationToken);
            TempData["Success"] = "План закрыт";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при закрытии плана {PlanId}", planId);
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Details), new { id = planId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CompleteOperation(
        int planId,
        int operationId,
        [ModelBinder(typeof(DecimalCommaModelBinder))] decimal actualHours,
        int? actualQuantity,
        string? returnUrl,
        CancellationToken cancellationToken)
    {
        try
        {
            await _planService.CompleteOperationAsync(planId, operationId, actualHours, actualQuantity, cancellationToken);
            TempData["Success"] = "Операция отмечена выполненной";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при отметке операции {OperationId}", operationId);
            TempData["Error"] = ex.Message;
        }

        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);

        return RedirectToAction(nameof(Details), new { id = planId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SkipOperation(
        int planId,
        int operationId,
        string reason,
        CancellationToken cancellationToken)
    {
        try
        {
            await _planService.SkipOperationAsync(planId, operationId, reason, cancellationToken);
            TempData["Success"] = "Операция пропущена";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при пропуске операции {OperationId}", operationId);
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Details), new { id = planId });
    }

    [HttpGet]
    public async Task<IActionResult> Consolidated(int id, CancellationToken cancellationToken)
    {
        try
        {
            return View(await _planService.GetByIdAsync(id, cancellationToken));
        }
        catch (KeyNotFoundException ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при загрузке сводного плана {Id}", id);
            TempData["Error"] = "Ошибка при загрузке сводного плана";
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpGet]
    public async Task<IActionResult> Workload(
        DateTime? from,
        DateTime? to,
        CancellationToken cancellationToken)
    {
        try
        {
            var start = (from ?? DateTime.Today.AddDays(-7)).Date;
            var end = (to ?? DateTime.Today).Date.AddDays(1);

            var workload = await _planService.GetWorkloadByBrigadeAsync(start, end, cancellationToken);
            ViewBag.From = start.ToString("yyyy-MM-dd");
            ViewBag.To = end.AddDays(-1).ToString("yyyy-MM-dd");
            return View(workload);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при расчёте трудоёмкости по бригадам");
            TempData["Error"] = "Ошибка при расчёте трудоёмкости";
            return View(new List<BrigadeWorkloadDto>());
        }
    }
}
