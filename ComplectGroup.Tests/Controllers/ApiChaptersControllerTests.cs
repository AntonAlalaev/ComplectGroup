using ComplectGroup.Application.DTOs;
using ComplectGroup.Application.Interfaces;
using ComplectGroup.Web.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace ComplectGroup.Tests.Controllers;

public class ApiChaptersControllerTests
{
    private readonly Mock<IChapterService> _service = new();
    private readonly ApiChaptersController _controller;

    public ApiChaptersControllerTests()
    {
        _controller = new ApiChaptersController(_service.Object, NullLogger<ApiChaptersController>.Instance);
    }

    [Fact]
    public async Task GetAll_ReturnsOk_WithChapters()
    {
        var chapters = new List<ChapterDto> { new() { Id = 1, Name = "A" } };
        _service.Setup(s => s.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(chapters);

        var result = await _controller.GetAll(default);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(chapters, ok.Value);
    }

    [Fact]
    public async Task GetById_Found_ReturnsOk()
    {
        var chapter = new ChapterDto { Id = 1, Name = "A" };
        _service.Setup(s => s.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(chapter);

        var result = await _controller.GetById(1, default);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(chapter, ok.Value);
    }

    [Fact]
    public async Task GetById_NotFound_Returns404()
    {
        _service.Setup(s => s.GetByIdAsync(1, It.IsAny<CancellationToken>())).ThrowsAsync(new KeyNotFoundException());

        var result = await _controller.GetById(1, default);

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task Create_WhitespaceName_ReturnsBadRequest()
    {
        var result = await _controller.Create(new CreateChapterRequest { Name = "   " }, default);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Create_Success_ReturnsCreated()
    {
        var chapter = new ChapterDto { Id = 1, Name = "A" };
        _service.Setup(s => s.CreateAsync("A", It.IsAny<CancellationToken>())).ReturnsAsync(chapter);

        var result = await _controller.Create(new CreateChapterRequest { Name = "A" }, default);

        var created = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(chapter, created.Value);
    }

    [Fact]
    public async Task Update_WhitespaceName_ReturnsBadRequest()
    {
        var result = await _controller.Update(1, new UpdateChapterRequest { Name = " " }, default);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Update_Success_ReturnsOk()
    {
        var chapter = new ChapterDto { Id = 1, Name = "B" };
        _service.Setup(s => s.UpdateAsync(1, "B", It.IsAny<CancellationToken>())).ReturnsAsync(chapter);

        var result = await _controller.Update(1, new UpdateChapterRequest { Name = "B" }, default);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(chapter, ok.Value);
    }

    [Fact]
    public async Task Update_NotFound_Returns404()
    {
        _service.Setup(s => s.UpdateAsync(1, "B", It.IsAny<CancellationToken>())).ThrowsAsync(new KeyNotFoundException());

        var result = await _controller.Update(1, new UpdateChapterRequest { Name = "B" }, default);

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task Delete_Success_ReturnsNoContent()
    {
        _service.Setup(s => s.DeleteAsync(1, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var result = await _controller.Delete(1, default);

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task Delete_NotFound_Returns404()
    {
        _service.Setup(s => s.DeleteAsync(1, It.IsAny<CancellationToken>())).ThrowsAsync(new KeyNotFoundException());

        var result = await _controller.Delete(1, default);

        Assert.IsType<NotFoundObjectResult>(result);
    }
}
