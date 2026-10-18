using MediatR;
using System.Text.Json.Serialization;
using ToDoList.Application.DTOs.Item;

namespace ToDoList.Application.Items.UpdateToggleActive;

public sealed record UpdateToggleActiveCommand(Guid Id): IRequest<ItemDTO>
{
    [JsonIgnore]
    public Guid Id { get; set; }
    public required bool Active { get; set; }
}
