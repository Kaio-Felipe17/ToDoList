using AutoMapper;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using ToDoList.Application.Exceptions;
using ToDoList.Application.Items.Create;
using ToDoList.Application.Items.GetAll;
using ToDoList.Application.Items.GetById;
using ToDoList.Application.Items.Update;
using ToDoList.Application.Items.Delete;
using ToDoList.Application.Items.UpdateIsCompleted;
using ToDoList.Application.Items.UpdateToggleActive;
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

    [Fact]
    public async Task GetByIdReturnsItemOrNullWhenMissing()
    {
        var item = await _repository.CreateAsync(Item.Create("Task", "Details"));
        var handler = new GetByIdHandler(_repository, _mapper);
        var result = await handler.Handle(new GetItemByIdQuery(item.Id), CancellationToken.None);
        Assert.NotNull(result);
        Assert.Equal(item.Id, result.Id);
        Assert.Equal("Details", result.Description);
        Assert.Null(await handler.Handle(new GetItemByIdQuery(Guid.NewGuid()), CancellationToken.None));
    }

    [Fact]
    public async Task UpdateChangesContentAndPreservesIdentityStateAndAudit()
    {
        var item = await _repository.CreateAsync(Item.Create("Task", "Original"));
        item.Active = false;
        item.IsCompleted = true;
        await _repository.UpdateAsync(item);
        var createdAt = item.CreatedAt;
        var result = await new UpdateItemHandler(_repository, _mapper).Handle(
            new UpdateItemCommand(item.Id) { Name = "Updated", Description = "Details" }, CancellationToken.None);
        Assert.NotNull(result);
        Assert.Equal(item.Id, result.Id);
        _context.ChangeTracker.Clear();
        var persisted = await _repository.GetAsync(item.Id);
        Assert.NotNull(persisted);
        Assert.Equal("Updated", persisted.Name);
        Assert.Equal("Details", persisted.Description);
        Assert.True(persisted.IsCompleted);
        Assert.False(persisted.Active);
        Assert.Equal(createdAt, persisted.CreatedAt);
        Assert.NotNull(persisted.UpdatedAt);
        Assert.False(persisted.IsDeleted);
        Assert.Null(persisted.DeletedAt);
    }

    [Fact]
    public async Task MissingItemUpdatesReturnNullAndDoNotCreateRecords()
    {
        var id = Guid.NewGuid();
        Assert.Null(await new UpdateItemHandler(_repository, _mapper).Handle(
            new UpdateItemCommand(id) { Name = "Updated" }, CancellationToken.None));
        Assert.Null(await new UpdateIsCompletedHandler(_repository, _mapper).Handle(
            new UpdateIsCompletedCommand(id) { IsCompleted = true }, CancellationToken.None));
        Assert.Null(await new UpdateToggleActiveHandler(_repository, _mapper).Handle(
            new UpdateToggleActiveCommand(id) { Active = false }, CancellationToken.None));
        Assert.Empty(await _context.Items.IgnoreQueryFilters().ToListAsync());
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task StateHandlersPersistRequestedValues(bool value)
    {
        var item = await _repository.CreateAsync(Item.Create("Task", "Details"));
        item.IsCompleted = !value;
        item.Active = !value;
        await _repository.UpdateAsync(item);
        var completion = await new UpdateIsCompletedHandler(_repository, _mapper).Handle(
            new UpdateIsCompletedCommand(item.Id) { IsCompleted = value }, CancellationToken.None);
        Assert.Equal(value, completion.IsCompleted);
        Assert.Equal(!value, completion.Active);
        var active = await new UpdateToggleActiveHandler(_repository, _mapper).Handle(
            new UpdateToggleActiveCommand(item.Id) { Active = value }, CancellationToken.None);
        Assert.Equal(value, active.Active);
        _context.ChangeTracker.Clear();
        var persisted = await _repository.GetAsync(item.Id);
        Assert.NotNull(persisted);
        Assert.Equal(value, persisted.IsCompleted);
        Assert.Equal(value, persisted.Active);
        Assert.Equal("Task", persisted.Name);
        Assert.Equal("Details", persisted.Description);
    }

    [Fact]
    public async Task DeleteHandlerSoftDeletesAndToleratesMissingItem()
    {
        var item = await _repository.CreateAsync(Item.Create("Task", null));
        var handler = new DeleteItemHandler(_repository);
        await handler.Handle(new DeleteItemCommand(item.Id), CancellationToken.None);
        await handler.Handle(new DeleteItemCommand(item.Id), CancellationToken.None);
        await handler.Handle(new DeleteItemCommand(Guid.NewGuid()), CancellationToken.None);
        _context.ChangeTracker.Clear();
        Assert.Null(await _repository.GetAsync(item.Id));
        var deleted = await _context.Items.IgnoreQueryFilters().SingleAsync();
        Assert.True(deleted.IsDeleted);
        Assert.False(deleted.Active);
        Assert.NotNull(deleted.DeletedAt);
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }
}
