using ComplectGroup.Application.DTOs;
using ComplectGroup.Application.Interfaces;
using ComplectGroup.Web.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace ComplectGroup.Tests.Controllers;

public class ApiPartsControllerTests
{
    private readonly Mock<IPartService> _service = new();
    private readonly ApiPartsController _controller;

    public ApiPartsControllerTests()
    {
        _controller = new ApiPartsController(_service.Object, NullLogger<ApiPartsController>.Instance);
    }

    [Fact]
    public async Task GetAll_ReturnsOk_WithParts()
    {
        var parts = new List<PartDto> { new() { Id = 1, Name = "P" } };
        _service.Setup(s => s.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(parts);

        var result = await _controller.GetAll(default);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(parts, ok.Value);
    }

    [Fact]
    public async Task GetById_Found_ReturnsOk()
    {
        var part = new PartDto { Id = 1, Name = "P" };
        _service.Setup(s => s.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(part);

        var result = await _controller.GetById(1, default);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(part, ok.Value);
    }

    [Fact]
    public async Task GetById_NotFound_Returns404()
    {
        _service.Setup(s => s.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync((PartDto?)null);

        var result = await _controller.GetById(1, default);

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task GetByChapter_ReturnsOk_WithParts()
    {
        var parts = new List<PartDto> { new() { Id = 1, Name = "P" } };
        _service.Setup(s => s.GetByChapterIdAsync(2, It.IsAny<CancellationToken>())).ReturnsAsync(parts);

        var result = await _controller.GetByChapter(2, default);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(parts, ok.Value);
    }

    [Fact]
    public async Task Create_InvalidRequest_ReturnsBadRequest()
    {
        var result = await _controller.Create(new CreatePartRequest { Name = "   ", ChapterId = 0 }, default);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Create_Success_ReturnsCreated()
    {
        var part = new PartDto { Id = 1, Name = "P" };
        _service.Setup(s => s.CreateAsync("P", 2, It.IsAny<CancellationToken>())).ReturnsAsync(part);

        var result = await _controller.Create(new CreatePartRequest { Name = "P", ChapterId = 2 }, default);

        var created = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(part, created.Value);
    }

    [Fact]
    public async Task Create_ChapterNotFound_Returns404()
    {
        _service.Setup(s => s.CreateAsync("P", 2, It.IsAny<CancellationToken>())).ThrowsAsync(new KeyNotFoundException("chapter"));

        var result = await _controller.Create(new CreatePartRequest { Name = "P", ChapterId = 2 }, default);

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task Update_InvalidRequest_ReturnsBadRequest()
    {
        var result = await _controller.Update(1, new UpdatePartRequest { Name = " ", ChapterId = 0 }, default);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Update_Success_ReturnsOk()
    {
        var part = new PartDto { Id = 1, Name = "P2" };
        _service.Setup(s => s.UpdateAsync(1, "P2", 2, It.IsAny<CancellationToken>())).ReturnsAsync(part);

        var result = await _controller.Update(1, new UpdatePartRequest { Name = "P2", ChapterId = 2 }, default);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(part, ok.Value);
    }

    [Fact]
    public async Task Update_NotFound_Returns404()
    {
        _service.Setup(s => s.UpdateAsync(1, "P2", 2, It.IsAny<CancellationToken>())).ThrowsAsync(new KeyNotFoundException());

        var result = await _controller.Update(1, new UpdatePartRequest { Name = "P2", ChapterId = 2 }, default);

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
