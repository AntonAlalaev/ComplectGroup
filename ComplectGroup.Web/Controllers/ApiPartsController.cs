using ComplectGroup.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ComplectGroup.Web.Controllers;

/// <summary>
/// REST API контроллер для управления деталями
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ApiPartsController : ControllerBase
{
    private readonly IPartService _partService;
    private readonly ILogger<ApiPartsController> _logger;

    public ApiPartsController(IPartService partService, ILogger<ApiPartsController> logger)
    {
        _partService = partService;
        _logger = logger;
    }

    /// <summary>
    /// Получить все детали
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        try
        {
            var parts = await _partService.GetAllAsync(cancellationToken);
            return Ok(parts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении списка деталей");
            return StatusCode(500, new { error = "Внутренняя ошибка сервера" });
        }
    }

    /// <summary>
    /// Получить деталь по ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        try
        {
            var part = await _partService.GetByIdAsync(id, cancellationToken);
            return Ok(part);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { error = $"Деталь с ID {id} не найдена" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении детали ID={Id}", id);
            return StatusCode(500, new { error = "Внутренняя ошибка сервера" });
        }
    }

    /// <summary>
    /// Получить детали по ID главы
    /// </summary>
    [HttpGet("by-chapter/{chapterId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByChapter(int chapterId, CancellationToken cancellationToken)
    {
        try
        {
            var parts = await _partService.GetByChapterIdAsync(chapterId, cancellationToken);
            return Ok(parts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении деталей главы ID={ChapterId}", chapterId);
            return StatusCode(500, new { error = "Внутренняя ошибка сервера" });
        }
    }

    /// <summary>
    /// Создать новую деталь
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreatePartRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name) || request.ChapterId <= 0)
            return BadRequest(new { error = "Название и ID главы обязательны" });

        try
        {
            var result = await _partService.CreateAsync(request.Name, request.ChapterId, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при создании детали");
            return StatusCode(500, new { error = "Внутренняя ошибка сервера" });
        }
    }

    /// <summary>
    /// Обновить деталь
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdatePartRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name) || request.ChapterId <= 0)
            return BadRequest(new { error = "Название и ID главы обязательны" });

        try
        {
            var result = await _partService.UpdateAsync(id, request.Name, request.ChapterId, cancellationToken);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при обновлении детали ID={Id}", id);
            return StatusCode(500, new { error = "Внутренняя ошибка сервера" });
        }
    }

    /// <summary>
    /// Удалить деталь
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        try
        {
            await _partService.DeleteAsync(id, cancellationToken);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { error = $"Деталь с ID {id} не найдена" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при удалении детали ID={Id}", id);
            return StatusCode(500, new { error = "Внутренняя ошибка сервера" });
        }
    }
}

// Request DTOs
public class CreatePartRequest
{
    public required string Name { get; set; }
    public int ChapterId { get; set; }
}

public class UpdatePartRequest
{
    public required string Name { get; set; }
    public int ChapterId { get; set; }
}
