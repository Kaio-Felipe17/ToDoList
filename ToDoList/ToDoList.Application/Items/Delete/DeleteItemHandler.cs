using MediatR;
using ToDoList.Application.Repositories;

namespace ToDoList.Application.Items.Delete;

public class DeleteItemHandler(IItemRepository repository) : IRequestHandler<DeleteItemCommand>
{
    public async Task Handle(DeleteItemCommand command, CancellationToken cancellationToken)
    {
        var item = await repository.GetAsync(command.Id, cancellationToken);
        if (item is null) return;
        await repository.DeleteAsync(item, cancellationToken);
    }
}
