using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using ToDoList.Application.Exceptions;
using ToDoList.Application.Repositories;
using ToDoList.Domain.Entities;
using ToDoList.Infrastructure.Common;
using ToDoList.Infrastructure.Context;

namespace ToDoList.Infrastructure.Repositories;

public class ItemRepository(ApiDbContext context) : BaseRepository<Item>(context), IItemRepository
{
    public Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default)
        => _context.Items.AnyAsync(item => item.Name == name, cancellationToken);

    public override async Task<Item> CreateAsync(Item entity, CancellationToken cancellationToken = default)
    {
        try
        {
            return await base.CreateAsync(entity, cancellationToken);
        }
        catch (DbUpdateException exception) when (exception.InnerException is SqlException { Number: 2601 or 2627 })
        {
            throw new ItemNameConflictException();
        }
    }
}