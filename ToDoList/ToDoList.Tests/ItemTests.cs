using AutoMapper;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using ToDoList.Application.Exceptions;
using ToDoList.Application.Items.Create;
using ToDoList.Application.Items.GetAll;
using ToDoList.Application.Mappings;
using ToDoList.Domain.Entities;
using ToDoList.Infrastructure.Context;
using ToDoList.Infrastructure.Repositories;

namespace ToDoList.Tests;

public sealed class ItemTests : IDisposable
{
    private readonly SqliteConnection _connection = new("Data Source=:memory:");
    private readonly ApiDbContext _context;
    private readonly ItemRepository _repository;
    private readonly IMapper _mapper;

    public ItemTests()
    {
        _connection.Open();
        _context = new ApiDbContext(new DbContextOptionsBuilder<ApiDbContext>()
            .UseSqlite(_connection).Options);
        _context.Database.EnsureCreated();
        _repository = new ItemRepository(_context);
        var configuration = new MapperConfiguration(cfg => cfg.AddProfile<ItemMappingProfile>(),
            NullLoggerFactory.Instance);
        configuration.AssertConfigurationIsValid();
        _mapper = configuration.CreateMapper();
    }

    [Fact]
    public async Task CreatePersistsNormalizedItemWithInitialStateAndAudit()
    {
        var handler = new CreateItemHandler(_repository, _mapper);
        var result = await handler.Handle(new CreateItemCommand
        {
            Name = "  Comprar pão  ", Description = "  Padaria  "
        }, CancellationToken.None);

        var persisted = await _context.Items.SingleAsync();
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal("Comprar pão", result.Name);
        Assert.Equal("Padaria", result.Description);
        Assert.True(result.Active);
        Assert.False(result.IsCompleted);
        Assert.NotNull(persisted.CreatedAt);
        Assert.False(persisted.IsDeleted);

        var list = await new GetAllItemsHandler(_repository, _mapper)
            .Handle(new GetAllItemsQuery(), CancellationToken.None);
        Assert.Equal(result.Id, Assert.Single(list).Id);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task InvalidNameDoesNotPersist(string? name)
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            new CreateItemHandler(_repository, _mapper).Handle(
                new CreateItemCommand { Name = name! }, CancellationToken.None));
        Assert.Empty(await _context.Items.ToListAsync());
    }

    [Fact]
    public void RejectsValuesAboveConfiguredLengths()
    {
        Assert.Throws<ArgumentException>(() => Item.Create(new string('n', 101), null));
        Assert.Throws<ArgumentException>(() => Item.Create("Valid", new string('d', 201)));
        Assert.Equal(100, Item.Create(new string('n', 100), new string('d', 200)).Name.Length);
    }

    [Fact]
    public async Task DuplicateNameProducesConflict()
    {
        var handler = new CreateItemHandler(_repository, _mapper);
        await handler.Handle(new CreateItemCommand { Name = "Task" }, CancellationToken.None);
        await Assert.ThrowsAsync<ItemNameConflictException>(() =>
            handler.Handle(new CreateItemCommand { Name = " Task " }, CancellationToken.None));
        Assert.Single(await _context.Items.ToListAsync());
    }

    [Fact]
    public async Task UniqueIndexRejectsDuplicateWhenApplicationCheckIsBypassed()
    {
        await _repository.CreateAsync(Item.Create("Task", null));
        await Assert.ThrowsAsync<DbUpdateException>(() =>
            _repository.CreateAsync(Item.Create("Task", null)));
    }

    [Fact]
    public async Task DeletePreservesRecordAndAllowsReusingName()
    {
        var item = await _repository.CreateAsync(Item.Create("Task", null));
        await _repository.DeleteAsync(item);

        Assert.Empty(await _repository.GetAllAsync());
        Assert.Null(await _repository.GetAsync(item.Id));
        Assert.False(await _repository.ExistsByNameAsync("Task"));
        var deleted = await _context.Items.IgnoreQueryFilters().SingleAsync();
        Assert.True(deleted.IsDeleted);
        Assert.False(deleted.Active);
        Assert.NotNull(deleted.DeletedAt);
        Assert.NotNull(deleted.UpdatedAt);

        await _repository.CreateAsync(Item.Create("Task", null));
        Assert.Single(await _repository.GetAllAsync());
        Assert.Equal(2, await _context.Items.IgnoreQueryFilters().CountAsync());
    }

    [Fact]
    public async Task UpdatePreservesCreationTimestamp()
    {
        var item = await _repository.CreateAsync(Item.Create("Task", null));
        var createdAt = item.CreatedAt;
        item.CreatedAt = DateTime.UtcNow.AddYears(-1);
        item.Description = "Updated";
        await _repository.UpdateAsync(item);
        _context.ChangeTracker.Clear();
        var persisted = await _repository.GetAsync(item.Id);
        Assert.Equal(createdAt, persisted!.CreatedAt);
        Assert.NotNull(persisted.UpdatedAt);
    }

    [Fact]
    public async Task CancellationReachesPersistence()
    {
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            new CreateItemHandler(_repository, _mapper).Handle(
                new CreateItemCommand { Name = "Task" }, cancellation.Token));
        Assert.Empty(await _context.Items.ToListAsync());
    }

    [Fact]
    public void ConfigurationIncludesLengthsAndUniqueFilteredIndex()
    {
        var entity = _context.Model.FindEntityType(typeof(Item))!;
        Assert.Equal(100, entity.FindProperty(nameof(Item.Name))!.GetMaxLength());
        Assert.Equal(200, entity.FindProperty(nameof(Item.Description))!.GetMaxLength());
        var index = Assert.Single(entity.GetIndexes());
        Assert.True(index.IsUnique);
        Assert.Equal("[IsDeleted] = 0", index.GetFilter());
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }
}