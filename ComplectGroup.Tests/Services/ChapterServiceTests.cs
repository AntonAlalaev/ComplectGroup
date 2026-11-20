using ComplectGroup.Application.DTOs;
using ComplectGroup.Application.Interfaces;
using ComplectGroup.Application.Services;
using ComplectGroup.Domain.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace ComplectGroup.Tests.Services;

public class ChapterServiceTests
{
    private readonly Mock<IChapterRepository> _repo = new();
    private readonly ChapterService _service;

    public ChapterServiceTests()
    {
        _service = new ChapterService(_repo.Object, NullLogger<ChapterService>.Instance);
    }

    [Fact]
    public async Task GetAll_ReturnsMappedChapters()
    {
        _repo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Chapter> { new() { Id = 1, Name = "A" } });

        var result = await _service.GetAllAsync(default);

        Assert.Single(result);
        Assert.Equal(1, result[0].Id);
        Assert.Equal("A", result[0].Name);
    }

    [Fact]
    public async Task GetById_Found_ReturnsDto()
    {
        _repo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Chapter { Id = 1, Name = "A" });

        var result = await _service.GetByIdAsync(1, default);

        Assert.NotNull(result);
        Assert.Equal("A", result!.Name);
    }

    [Fact]
    public async Task GetById_NotFound_ThrowsKeyNotFound()
    {
        _repo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync((Chapter?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.GetByIdAsync(1, default));
    }

    [Fact]
    public async Task Create_Whitespace_ThrowsArgument()
    {
        await Assert.ThrowsAsync<ArgumentException>(() => _service.CreateAsync("   ", default));
    }

    [Fact]
    public async Task Create_Valid_AddsAndReturnsDto()
    {
        _repo.Setup(r => r.AddAsync(It.IsAny<Chapter>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var result = await _service.CreateAsync("New", default);

        Assert.Equal("New", result.Name);
        _repo.Verify(r => r.AddAsync(It.Is<Chapter>(c => c.Name == "New"), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Update_Whitespace_ThrowsArgument()
    {
        await Assert.ThrowsAsync<ArgumentException>(() => _service.UpdateAsync(1, " ", default));
    }

    [Fact]
    public async Task Update_NotFound_ThrowsKeyNotFound()
    {
        _repo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync((Chapter?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.UpdateAsync(1, "X", default));
    }

    [Fact]
    public async Task Update_Valid_UpdatesAndReturnsDto()
    {
        var chapter = new Chapter { Id = 1, Name = "Old" };
        _repo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(chapter);
        _repo.Setup(r => r.UpdateAsync(It.IsAny<Chapter>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var result = await _service.UpdateAsync(1, "New", default);

        Assert.Equal("New", result.Name);
        Assert.Equal("New", chapter.Name);
    }

    [Fact]
    public async Task Delete_NotFound_ThrowsKeyNotFound()
    {
        _repo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync((Chapter?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.DeleteAsync(1, default));
    }

    [Fact]
    public async Task Delete_Found_Deletes()
    {
        var chapter = new Chapter { Id = 1, Name = "A" };
        _repo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(chapter);
        _repo.Setup(r => r.DeleteAsync(chapter, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        await _service.DeleteAsync(1, default);

        _repo.Verify(r => r.DeleteAsync(chapter, It.IsAny<CancellationToken>()), Times.Once);
    }
}
