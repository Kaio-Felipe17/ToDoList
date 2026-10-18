using MediatR;
using System.Text.Json.Serialization;
using ToDoList.Application.DTOs.Item;

namespace ToDoList.Application.Items.UpdateToggleActive;

public sealed record UpdateToggleActiveCommand([property: JsonIgnore] Guid Id): IRequest<ItemDTO>
{
    public required bool Active { get; set; }
}
