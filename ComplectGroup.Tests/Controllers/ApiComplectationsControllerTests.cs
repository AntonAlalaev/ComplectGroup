using ComplectGroup.Application.DTOs;
using ComplectGroup.Application.Interfaces;
using ComplectGroup.Application.Models;
using ComplectGroup.Web.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace ComplectGroup.Tests.Controllers;

public class ApiComplectationsControllerTests
{
    private readonly Mock<IComplectationService> _service = new();
    private readonly ApiComplectationsController _controller;

    public ApiComplectationsControllerTests()
    {
        _controller = new ApiComplectationsController(_service.Object, NullLogger<ApiComplectationsController>.Instance);
    }

    [Fact]
    public async Task GetAll_ReturnsOk_WithComplectations()
    {
        var list = new List<ComplectationDto> { new() { Id = 1, Number = "C1" } };
        _service.Setup(s => s.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(list);

        var result = await _controller.GetAll(default);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(list, ok.Value);
    }

    [Fact]
    public async Task GetById_Found_ReturnsOk()
    {
        var c = new ComplectationDto { Id = 1, Number = "C1" };
        _service.Setup(s => s.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(c);

        var result = await _controller.GetById(1, default);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(c, ok.Value);
    }

    [Fact]
    public async Task GetById_NotFound_Returns404()
    {
        _service.Setup(s => s.GetByIdAsync(1, It.IsAny<CancellationToken>())).ThrowsAsync(new KeyNotFoundException());

        var result = await _controller.GetById(1, default);

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task Create_Success_ReturnsCreated()
    {
        var c = new ComplectationDto { Id = 5, Number = "C1" };
        _service.Setup(s => s.CreateAsync(It.IsAny<CreateComplectationRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(c);

        var result = await _controller.Create(new CreateComplectationRequest { Number = "C1" }, default);

        var created = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(c, created.Value);
    }

    [Fact]
    public async Task Create_InvalidModelState_ReturnsBadRequest()
    {
        _controller.ModelState.AddModelError("Number", "Required");

        var result = await _controller.Create(new CreateComplectationRequest(), default);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Create_ArgumentException_ReturnsBadRequest()
    {
        _service.Setup(s => s.CreateAsync(It.IsAny<CreateComplectationRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ArgumentException("bad"));

        var result = await _controller.Create(new CreateComplectationRequest { Number = "C1" }, default);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Create_KeyNotFoundException_ReturnsNotFound()
    {
        _service.Setup(s => s.CreateAsync(It.IsAny<CreateComplectationRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new KeyNotFoundException("missing"));

        var result = await _controller.Create(new CreateComplectationRequest { Number = "C1" }, default);

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task Update_Success_ReturnsOk()
    {
        _service.Setup(s => s.UpdateAsync(1, It.IsAny<UpdateComplectationRequest>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _controller.Update(1, new UpdateComplectationRequest { Number = "C2" }, default);

        Assert.IsType<OkResult>(result);
    }

    [Fact]
    public async Task Update_InvalidModelState_ReturnsBadRequest()
    {
        _controller.ModelState.AddModelError("Number", "bad");

        var result = await _controller.Update(1, new UpdateComplectationRequest(), default);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Update_NotFound_Returns404()
    {
        _service.Setup(s => s.UpdateAsync(1, It.IsAny<UpdateComplectationRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new KeyNotFoundException());

        var result = await _controller.Update(1, new UpdateComplectationRequest(), default);

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

    [Fact]
    public async Task Search_ReturnsOk_WithPagedResult()
    {
        var paged = new PagedComplectationsResult
        {
            Items = new List<ComplectationDto> { new() { Id = 1, Number = "C1" } },
            TotalCount = 1,
            PageNumber = 1,
            PageSize = 20
        };
        _service.Setup(s => s.GetFilteredAsync(It.IsAny<ComplectationFilterViewModel>(), It.IsAny<CancellationToken>())).ReturnsAsync(paged);

        var result = await _controller.Search(null, null, null, null, null, null, null, 1, 20, default);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(ok.Value);
    }
}
