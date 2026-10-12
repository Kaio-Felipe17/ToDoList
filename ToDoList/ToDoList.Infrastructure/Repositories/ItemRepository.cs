using ToDoList.Application.Entities;
using ToDoList.Application.Repositories;
using ToDoList.Infrastructure.Common;
using ToDoList.Infrastructure.Context;

namespace ToDoList.Infrastructure.Repositories;

public class ItemRepository : BaseRepository<Item>, IItemRepository
{
    private readonly ApiDbContext _context;

    public ItemRepository(ApiDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<bool> ExistsByNameAsync(string name)
    {
        var items = await GetAllAsync();
        return items.Any(x => x.Name == name);
    }
}
