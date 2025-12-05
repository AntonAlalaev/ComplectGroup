using ComplectGroup.Application.DTOs;
using ComplectGroup.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ComplectGroup.Web.Controllers;

public class PartsController : Controller
{
    private readonly IPartService _partService;
    private readonly IChapterService _chapterService;
    private readonly ILogger<PartsController> _logger;

    public PartsController(
        IPartService partService,
        IChapterService chapterService,
        ILogger<PartsController> logger)
    {
        _partService = partService;
        _chapterService = chapterService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        try
        {
            var parts = await _partService.GetAllAsync(cancellationToken);
            return View(parts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при загрузке деталей");
            return View(new List<PartDto>());
        }
    }

    [HttpGet]
    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        try
        {
            var chapters = await _chapterService.GetAllAsync(cancellationToken);
            ViewBag.Chapters = chapters;
            return View();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при загрузке глав");
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost]
    public async Task<IActionResult> Create(string name, int chapterId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(name) || chapterId <= 0)
        {
            ModelState.AddModelError("", "Все поля обязательны");
            return View();
        }

        try
        {
            await _partService.CreateAsync(name, chapterId, cancellationToken);
            TempData["Success"] = "Деталь успешно создана";
            return RedirectToAction(nameof(Index));
        }
        catch (KeyNotFoundException ex)
        {
            ModelState.AddModelError("", ex.Message);
            return View();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при создании детали");
            ModelState.AddModelError("", "Ошибка при сохранении");
            return View();
        }
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        try
        {
            await _partService.DeleteAsync(id, cancellationToken);
            TempData["Success"] = "Деталь успешно удалена";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при удалении детали");
            TempData["Error"] = "Ошибка при удалении";
        }

        return RedirectToAction(nameof(Index));
    }
}
