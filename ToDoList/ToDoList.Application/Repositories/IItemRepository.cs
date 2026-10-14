using ToDoList.Domain.Entities;

namespace ToDoList.Application.Repositories;

public interface IItemRepository : IRepository<Item>
{
    Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default);
}