using ComplectGroup.Domain.Entities;
using ComplectGroup.Infrastructure.Data;
using ComplectGroup.Infrastructure.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace ComplectGroup.Tests.Repositories;

public class ChapterRepositoryTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly AppDbContext _context;
    private readonly ChapterRepository _repository;

    public ChapterRepositoryTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .Options;

        _context = new AppDbContext(options);
        _context.Database.EnsureCreated();

        _repository = new ChapterRepository(_context);
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }

    [Fact]
    public async Task AddAsync_ThenGetAll_ReturnsChapter()
    {
        await _repository.AddAsync(new Chapter { Name = "A" }, default);

        var chapters = await _repository.GetAllAsync(default);

        Assert.Single(chapters);
        Assert.Equal("A", chapters[0].Name);
    }

    [Fact]
    public async Task GetByIdAsync_Existing_ReturnsChapter()
    {
        await _repository.AddAsync(new Chapter { Name = "A" }, default);
        var added = (await _repository.GetAllAsync(default))[0];

        var chapter = await _repository.GetByIdAsync(added.Id, default);

        Assert.NotNull(chapter);
        Assert.Equal("A", chapter!.Name);
    }

    [Fact]
    public async Task GetByIdAsync_Missing_ReturnsNull()
    {
        var chapter = await _repository.GetByIdAsync(999, default);

        Assert.Null(chapter);
    }

    [Fact]
    public async Task UpdateAsync_ChangesName()
    {
        await _repository.AddAsync(new Chapter { Name = "Old" }, default);
        var added = (await _repository.GetAllAsync(default))[0];
        var chapter = await _repository.GetByIdAsync(added.Id, default);
        chapter!.Name = "New";

        await _repository.UpdateAsync(chapter, default);

        var updated = await _repository.GetByIdAsync(chapter.Id, default);
        Assert.Equal("New", updated!.Name);
    }

    [Fact]
    public async Task DeleteAsync_RemovesChapter()
    {
        await _repository.AddAsync(new Chapter { Name = "A" }, default);
        var added = (await _repository.GetAllAsync(default))[0];
        var chapter = await _repository.GetByIdAsync(added.Id, default);

        await _repository.DeleteAsync(chapter!, default);

        Assert.Empty(await _repository.GetAllAsync(default));
    }

}
