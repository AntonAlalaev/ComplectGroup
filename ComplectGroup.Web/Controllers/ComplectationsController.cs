using ComplectGroup.Application.DTOs;
using ComplectGroup.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ComplectGroup.Web.Controllers;

public class ComplectationsController : Controller
{
    private readonly IComplectationService _complectationService;
    private readonly ILogger<ComplectationsController> _logger;

    public ComplectationsController(
        IComplectationService complectationService,
        ILogger<ComplectationsController> logger)
    {
        _complectationService = complectationService;
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
                TotalVolume = dto.TotalVolume
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
}
