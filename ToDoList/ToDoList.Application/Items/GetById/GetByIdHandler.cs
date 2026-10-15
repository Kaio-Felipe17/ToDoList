using AutoMapper;
using MediatR;
using ToDoList.Application.DTOs.Item;
using ToDoList.Application.Repositories;

namespace ToDoList.Application.Items.GetById;

public sealed class GetByIdHandler(IItemRepository repository, IMapper mapper) 
    : IRequestHandler<GetItemByIdQuery, ItemDTO?>
{
    public async Task<ItemDTO?> Handle(GetItemByIdQuery query, CancellationToken cancellationToken)
    {
        var item = await repository.GetAsync(query.Id, cancellationToken);
        return item is null ? null : mapper.Map<ItemDTO>(item);
    }
}
