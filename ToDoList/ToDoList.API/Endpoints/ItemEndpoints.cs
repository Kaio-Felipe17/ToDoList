using MediatR;
using Microsoft.AspNetCore.Mvc;
using ToDoList.Application.Items.Create;
using ToDoList.Application.Items.GetAll;

namespace ToDoList.API.Endpoints;

public static class ItemEndpoints
{
    public static void MapItemEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/items")
            .WithTags("Items");

        group.MapPost("", CreateItem);
        group.MapGet("", GetAllItems);
    }

    public static async Task<IResult> CreateItem([FromBody] CreateItemCommand command, IMediator mediator, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(command, cancellationToken);
        return Results.Created((string?)null, result);
    }

    public static async Task<IResult> GetAllItems(IMediator mediator, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetAllItemsQuery(), cancellationToken);
        return Results.Ok(result);
    }
}
