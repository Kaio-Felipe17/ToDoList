using AutoMapper;
using MediatR;
using ToDoList.Application.DTOs.Item;
using ToDoList.Application.Repositories;

namespace ToDoList.Application.Items.UpdateToggleActive;

public class UpdateToggleActiveHandler(IItemRepository repository, IMapper mapper) : IRequestHandler<UpdateToggleActiveCommand, ItemDTO>
{
    public async Task<ItemDTO> Handle(UpdateToggleActiveCommand request, CancellationToken cancellationToken)
    {
        var item = await repository.GetAsync(request.Id, cancellationToken);
        if (item is null) return null!;
        item.Active = request.Active;
        await repository.UpdateAsync(item, cancellationToken);
        return mapper.Map<ItemDTO>(item);
    }
}
