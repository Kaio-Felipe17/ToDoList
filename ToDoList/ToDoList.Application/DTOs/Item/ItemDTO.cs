namespace ToDoList.Application.DTOs.Item;

public class ItemDTO
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string? Description { get; set; }
    public bool IsCompleted { get; set; }
    public bool Active { get; set; }
}
