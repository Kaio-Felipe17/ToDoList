using MediatR;
using System.Text.Json.Serialization;
using ToDoList.Application.DTOs.Item;

namespace ToDoList.Application.Items.UpdateIsCompleted;

public sealed record UpdateIsCompletedCommand(Guid Id) : IRequest<ItemDTO>
{
    [JsonIgnore]
    public Guid Id { get; set; }
    public required bool IsCompleted { get; set; }
}
