using ComplectGroup.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ComplectGroup.Web.Controllers;

/// <summary>
/// REST API контроллер для управления разделами комплектации
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ApiChaptersController : ControllerBase
{
    /// <summary>
    /// Сервис для работы с разделами комплектации
    /// </summary>
    private readonly IChapterService _chapterService;

    /// <summary>
    /// Логгер
    /// </summary>
    private readonly ILogger<ApiChaptersController> _logger;

    /// <summary>
    /// Конструктор контроллера для работы с разделами комплектации
    /// </summary>
    /// <param name="chapterService"></param>
    /// <param name="logger"></param>
    public ApiChaptersController(IChapterService chapterService, ILogger<ApiChaptersController> logger)
    {
        _chapterService = chapterService;
        _logger = logger;
    }

    /// <summary>
    /// Получить все разделы комплектации
    /// </summary>
    /// <response code="200">Список глав успешно получен</response>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        try
        {
            var chapters = await _chapterService.GetAllAsync(cancellationToken);
            return Ok(chapters);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении списка глав");
            return StatusCode(500, new { error = "Внутренняя ошибка сервера" });
        }
    }

    /// <summary>
    /// Получить раздел комплектации по ID
    /// </summary>
    /// <param name="id">ID главы</param>
    /// <response code="200">Глава найдена</response>
    /// <response code="404">Глава не найдена</response>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        try
        {
            var chapter = await _chapterService.GetByIdAsync(id, cancellationToken);
            return Ok(chapter);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { error = $"Глава с ID {id} не найдена" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении главы ID={Id}", id);
            return StatusCode(500, new { error = "Внутренняя ошибка сервера" });
        }
    }

    /// <summary>
    /// Создать новый раздел комплектации по названию
    /// </summary>
    /// <param name="request">Объект с названием главы</param>
    /// <response code="201">Глава успешно создана</response>
    /// <response code="400">Некорректные данные</response>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateChapterRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest(new { error = "Название главы обязательно" });

        try
        {
            var result = await _chapterService.CreateAsync(request.Name, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при создании главы");
            return StatusCode(500, new { error = "Внутренняя ошибка сервера" });
        }
    }

    /// <summary>
    /// Обновить раздел комплектации по ID
    /// </summary>
    /// <param name="id">ID главы</param>
    /// <param name="request">Обновленные данные</param>
    /// <response code="200">Глава успешно обновлена</response>
    /// <response code="404">Глава не найдена</response>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateChapterRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest(new { error = "Название главы обязательно" });

        try
        {
            var result = await _chapterService.UpdateAsync(id, request.Name, cancellationToken);
            return Ok(result);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { error = $"Глава с ID {id} не найдена" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при обновлении главы ID={Id}", id);
            return StatusCode(500, new { error = "Внутренняя ошибка сервера" });
        }
    }

    /// <summary>
    /// Удалить раздел комплектации по ID
    /// </summary>
    /// <param name="id">ID главы</param>
    /// <response code="204">Глава успешно удалена</response>
    /// <response code="404">Глава не найдена</response>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        try
        {
            await _chapterService.DeleteAsync(id, cancellationToken);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { error = $"Глава с ID {id} не найдена" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при удалении главы ID={Id}", id);
            return StatusCode(500, new { error = "Внутренняя ошибка сервера" });
        }
    }
}

//
// Request DTOs
//

/// <summary>
/// Создать новый раздел комплектации
/// </summary>
public class CreateChapterRequest
{
    public required string Name { get; set; }
}

/// <summary>
/// Обновить раздел комплектации
/// </summary>
public class UpdateChapterRequest
{
    public required string Name { get; set; }
}
