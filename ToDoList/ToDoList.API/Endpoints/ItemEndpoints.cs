using MediatR;
using ToDoList.Application.Commands.Item;

namespace ToDoList.API.Endpoints;

public static class ItemEndpoints
{
    public static void MapItemEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/items")
            .WithTags("Items");

        group.MapPost("", CreateItem);
    }

    public static async Task<IResult> CreateItem(CreateItemCommand command, IMediator mediator, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(command);
        return Results.Ok(result);
    }
}
