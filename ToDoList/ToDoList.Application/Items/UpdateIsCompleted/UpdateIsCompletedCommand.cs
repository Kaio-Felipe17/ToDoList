using MediatR;
using System.Text.Json.Serialization;
using ToDoList.Application.DTOs.Item;

namespace ToDoList.Application.Items.UpdateIsCompleted;

public sealed record UpdateIsCompletedCommand([property: JsonIgnore]Guid Id) : IRequest<ItemDTO>
{
    public required bool IsCompleted { get; set; }
}
