using ComplectGroup.Application.DTOs;
using ComplectGroup.Application.Interfaces;
using ComplectGroup.Application.Models;
using ComplectGroup.Domain.Entities;
using ComplectGroup.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ComplectGroup.Web.Controllers;

/// <summary>
/// REST API контроллер для управления комплектациями
/// </summary>
[ApiController]
[Route("api/complectations")]
[Produces("application/json")]
public class ApiComplectationsController : ControllerBase
{
    private readonly IComplectationService _complectationService;
    private readonly ILogger<ApiComplectationsController> _logger;

    public ApiComplectationsController(
        IComplectationService complectationService,
        ILogger<ApiComplectationsController> logger)
    {
        _complectationService = complectationService;
        _logger = logger;
    }

    /// <summary>
    /// Получить все комплектации
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        try
        {
            var complectations = await _complectationService.GetAllAsync(cancellationToken);
            return Ok(complectations);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении списка комплектаций");
            return StatusCode(500, new { error = "Внутренняя ошибка сервера" });
        }
    }

    /// <summary>
    /// Поиск комплектаций с фильтрами (для модального окна выбора)
    /// </summary>
    [HttpGet("search")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [AllowAnonymous]
    public async Task<IActionResult> Search(
        [FromQuery] string? searchNumber,
        [FromQuery] string? searchCustomer,
        [FromQuery] string? searchManager,
        [FromQuery] ComplectationStatus? status,
        [FromQuery] DateOnly? dateFrom,
        [FromQuery] DateOnly? dateTo,
        [FromQuery] string? preset,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var filter = new ComplectationFilterViewModel
            {
                SearchNumber = searchNumber,
                SearchCustomer = searchCustomer,
                SearchManager = searchManager,
                SearchAddress = null,
                DateFrom = dateFrom,
                DateTo = dateTo,
                Status = status,
                IsIgnored = null,
                IsFullyShipped = null,
                Preset = preset,
                SortBy = "Number",
                SortDescending = false,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            var result = await _complectationService.GetFilteredAsync(filter, cancellationToken);
            
            return Ok(new
            {
                items = result.Items,
                totalCount = result.TotalCount,
                pageNumber = result.PageNumber,
                pageSize = result.PageSize,
                totalPages = result.TotalPages,
                hasPrevious = result.HasPrevious,
                hasNext = result.HasNext
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при поиске комплектаций");
            return StatusCode(500, new { error = "Внутренняя ошибка сервера" });
        }
    }

    /// <summary>
    /// Получить комплектацию по ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        try
        {
            var complectation = await _complectationService.GetByIdAsync(id, cancellationToken);
            return Ok(complectation);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { error = $"Комплектация с ID {id} не найдена" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении комплектации ID={Id}", id);
            return StatusCode(500, new { error = "Внутренняя ошибка сервера" });
        }
    }

    /// <summary>
    /// Создать новую комплектацию
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] CreateComplectationRequest request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var result = await _complectationService.CreateAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при создании комплектации");
            return StatusCode(500, new { error = "Внутренняя ошибка сервера" });
        }
    }

    /// <summary>
    /// Обновить комплектацию
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateComplectationRequest request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            await _complectationService.UpdateAsync(id, request, cancellationToken); // Используем UpdateAsync вместо Update
            return Ok(); // или return NoContent(); если не нужно возвращать данные
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { error = $"Комплектация с ID {id} не найдена" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при обновлении комплектации ID={Id}", id);
            return StatusCode(500, new { error = "Внутренняя ошибка сервера" });
        }
    }

    /// <summary>
    /// Удалить комплектацию
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        try
        {
            await _complectationService.DeleteAsync(id, cancellationToken);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { error = $"Комплектация с ID {id} не найдена" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при удалении комплектации ID={Id}", id);
            return StatusCode(500, new { error = "Внутренняя ошибка сервера" });
        }
    }
}
