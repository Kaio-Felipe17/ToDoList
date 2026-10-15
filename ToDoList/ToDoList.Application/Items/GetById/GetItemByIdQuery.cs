using MediatR;
using ToDoList.Application.DTOs.Item;

namespace ToDoList.Application.Items.GetById;

public sealed record GetItemByIdQuery(Guid Id) : IRequest<ItemDTO?>;
