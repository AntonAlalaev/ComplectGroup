using ComplectGroup.Application.DTOs;
using ComplectGroup.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ComplectGroup.Web.Controllers;

/// <summary>
/// MVC контроллер для управления главами через web интерфейс
/// </summary>
public class ChaptersController : Controller
{
    private readonly IChapterService _service;
    private readonly ILogger<ChaptersController> _logger;

    public ChaptersController(IChapterService service, ILogger<ChaptersController> logger)
    {
        _service = service;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        try
        {
            var chapters = await _service.GetAllAsync(cancellationToken);
            return View(chapters);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при загрузке списка глав");
            TempData["Error"] = "Ошибка при загрузке данных";
            return View(new List<ChapterDto>());
        }
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Create(string name, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            ModelState.AddModelError("name", "Название обязательно");
            return View();
        }

        try
        {
            await _service.CreateAsync(name, cancellationToken);
            TempData["Success"] = "Глава успешно создана";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при создании главы");
            ModelState.AddModelError("", "Ошибка при сохранении");
            return View();
        }
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var chapter = await _service.GetByIdAsync(id, cancellationToken);
        ViewBag.Id = id;
        return View(chapter!.Name); // модель = string
    }

    [HttpPost]
    public async Task<IActionResult> Edit(int id, string model, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(model))
        {
            ModelState.AddModelError("", "Название обязательно");
            ViewBag.Id = id;
            return View(model);
        }

        await _service.UpdateAsync(id, model, cancellationToken);
        TempData["Success"] = "Глава успешно обновлена";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        try
        {
            await _service.DeleteAsync(id, cancellationToken);
            TempData["Success"] = "Глава успешно удалена";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при удалении главы");
            TempData["Error"] = "Ошибка при удалении";
        }

        return RedirectToAction(nameof(Index));
    }
}
