using MediatR;
using ToDoList.Application.DTOs.Item;

namespace ToDoList.Application.Items.Update;

public sealed record UpdateItemCommand(Guid Id) : IRequest<ItemDTO?>
{
    public required string Name { get; set; }
    public string? Description { get; set; }
}
