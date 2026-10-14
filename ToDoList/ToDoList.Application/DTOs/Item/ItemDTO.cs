namespace ToDoList.Application.DTOs.Item;

public class ItemDTO
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public bool IsCompleted { get; set; }
    public bool Active { get; set; }
}
