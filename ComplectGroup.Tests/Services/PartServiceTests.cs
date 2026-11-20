using ComplectGroup.Application.DTOs;
using ComplectGroup.Application.Interfaces;
using ComplectGroup.Application.Services;
using ComplectGroup.Domain.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace ComplectGroup.Tests.Services;

public class PartServiceTests
{
    private readonly Mock<IPartRepository> _partRepo = new();
    private readonly Mock<IChapterRepository> _chapterRepo = new();
    private readonly PartService _service;

    public PartServiceTests()
    {
        _service = new PartService(_partRepo.Object, _chapterRepo.Object, NullLogger<PartService>.Instance);
    }

    [Fact]
    public async Task GetById_Found_ReturnsDto()
    {
        _partRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Part { Id = 1, Name = "P", Chapter = new Chapter { Id = 2, Name = "C" } });

        var result = await _service.GetByIdAsync(1, default);

        Assert.NotNull(result);
        Assert.Equal("P", result!.Name);
        Assert.Equal(2, result.Chapter.Id);
    }

    [Fact]
    public async Task GetById_NotFound_ThrowsKeyNotFound()
    {
        _partRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync((Part?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.GetByIdAsync(1, default));
    }

    [Fact]
    public async Task Create_Whitespace_ThrowsArgument()
    {
        await Assert.ThrowsAsync<ArgumentException>(() => _service.CreateAsync(" ", 1, default));
    }

    [Fact]
    public async Task Create_ChapterNotFound_ThrowsKeyNotFound()
    {
        _chapterRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync((Chapter?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.CreateAsync("P", 1, default));
    }

    [Fact]
    public async Task Create_Valid_CreatesPart()
    {
        var chapter = new Chapter { Id = 1, Name = "C" };
        _chapterRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(chapter);
        _partRepo.Setup(r => r.AddAsync(It.IsAny<Part>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var result = await _service.CreateAsync("P", 1, default);

        Assert.Equal("P", result.Name);
        _partRepo.Verify(r => r.AddAsync(It.Is<Part>(p => p.Name == "P" && p.Chapter == chapter), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Update_NotFound_ThrowsKeyNotFound()
    {
        _partRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync((Part?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.UpdateAsync(1, "P", 1, default));
    }

    [Fact]
    public async Task Update_ChapterNotFound_ThrowsKeyNotFound()
    {
        var part = new Part { Id = 1, Name = "P", Chapter = new Chapter { Id = 1, Name = "C" } };
        _partRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(part);
        _chapterRepo.Setup(r => r.GetByIdAsync(2, It.IsAny<CancellationToken>())).ReturnsAsync((Chapter?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.UpdateAsync(1, "P", 2, default));
    }

    [Fact]
    public async Task Delete_NotFound_ThrowsKeyNotFound()
    {
        _partRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync((Part?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.DeleteAsync(1, default));
    }
}
