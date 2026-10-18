using AutoMapper;
using MediatR;
using ToDoList.Application.DTOs.Item;
using ToDoList.Application.Repositories;

namespace ToDoList.Application.Items.UpdateIsCompleted;

public class UpdateIsCompletedHandler(IItemRepository repository, IMapper mapper) : IRequestHandler<UpdateIsCompletedCommand, ItemDTO>
{
    public async Task<ItemDTO> Handle(UpdateIsCompletedCommand request, CancellationToken cancellationToken)
    {
        var item = await repository.GetAsync(request.Id, cancellationToken);
        if (item is null)
            return null!;
        item.IsCompleted = request.IsCompleted;
        await repository.UpdateAsync(item, cancellationToken);
        return mapper.Map<ItemDTO>(item);
    }
}
