using MediatR;

namespace ToDoList.Application.Items.Delete;

public sealed record DeleteItemCommand(Guid Id): IRequest;
