using AutoMapper;
using MediatR;
using ToDoList.Application.DTOs.Item;
using ToDoList.Application.Exceptions;
using ToDoList.Application.Repositories;
using ToDoList.Domain.Entities;

namespace ToDoList.Application.Items.Create;

public sealed class CreateItemHandler(IItemRepository repository, IMapper mapper)
    : IRequestHandler<CreateItemCommand, ItemDTO>
{
    public async Task<ItemDTO> Handle(CreateItemCommand command, CancellationToken cancellationToken)
    {
        var item = Item.Create(command.Name, command.Description);
        if (await repository.ExistsByNameAsync(item.Name, cancellationToken))
            throw new ItemNameConflictException();

        await repository.CreateAsync(item, cancellationToken);
        return mapper.Map<ItemDTO>(item);
    }
}