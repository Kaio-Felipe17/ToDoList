using AutoMapper;
using MediatR;
using ToDoList.Application.DTOs.Item;
using ToDoList.Application.Repositories;

namespace ToDoList.Application.Items.GetAll;

public sealed class GetAllItemsHandler(IItemRepository repository, IMapper mapper)
    : IRequestHandler<GetAllItemsQuery, List<ItemDTO>>
{
    public async Task<List<ItemDTO>> Handle(GetAllItemsQuery query, CancellationToken cancellationToken)
    {
        var items = await repository.GetAllAsync(cancellationToken);
        return mapper.Map<List<ItemDTO>>(items);
    }
}