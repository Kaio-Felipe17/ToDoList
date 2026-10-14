using MediatR;
using ToDoList.Application.DTOs.Item;

namespace ToDoList.Application.Items.Create;

public record CreateItemCommand : IRequest<ItemDTO>
{
    public required string Name { get; set; }
    public string? Description { get; set; }
}
