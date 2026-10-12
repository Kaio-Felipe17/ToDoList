namespace ToDoList.Application.Repositories;

public interface IItemRepository
{
    Task<bool> ExistsByNameAsync(string name);
}
