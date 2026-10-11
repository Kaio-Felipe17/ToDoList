using ToDoList.Application.Common;

namespace ToDoList.Application.Entities;

public class Item : BaseEntity
{
    public required string Name { get; set; }
    public string? Description { get; set; }
    public required bool IsCompleted { get; set; }
}
