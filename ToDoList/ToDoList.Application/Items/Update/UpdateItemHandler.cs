using AutoMapper;
using MediatR;
using ToDoList.Application.DTOs.Item;
using ToDoList.Application.Repositories;

namespace ToDoList.Application.Items.Update;

public class UpdateItemHandler(IItemRepository repository, IMapper mapper) 
    : IRequestHandler<UpdateItemCommand, ItemDTO?>
{
    public async Task<ItemDTO?> Handle(UpdateItemCommand command, CancellationToken cancellationToken)
    {
        var item = await repository.GetAsync(command.Id, cancellationToken);
        if (item is null) return null;
        mapper.Map(command, item);
        await repository.UpdateAsync(item, cancellationToken);
        return mapper.Map<ItemDTO>(item);
    }
}
