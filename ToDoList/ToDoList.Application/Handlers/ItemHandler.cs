using MediatR;
using ToDoList.Application.Commands.Item;
using ToDoList.Application.DTOs.Item;
using ToDoList.Application.Repositories;

namespace ToDoList.Application.Handlers;

public class ItemHandler : IRequestHandler<CreateItemCommand, ItemDTO>
{
    private readonly IItemRepository _itemRepository;

    public ItemHandler(IItemRepository itemRepository)
    {
        _itemRepository = itemRepository;
    }

    public async Task Handle(CreateItemCommand command)
    {
        var existsByName = await _itemRepository.ExistsByNameAsync(command.Name);
        if (existsByName) throw new Exception("Já existe um item cadastrado com esse nome");
    }
}
