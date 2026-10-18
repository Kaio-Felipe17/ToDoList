using MediatR;
using Microsoft.AspNetCore.Mvc;
using ToDoList.Application.Items.Create;
using ToDoList.Application.Items.Delete;
using ToDoList.Application.Items.GetAll;
using ToDoList.Application.Items.GetById;
using ToDoList.Application.Items.Update;
using ToDoList.Application.Items.UpdateIsCompleted;
using ToDoList.Application.Items.UpdateToggleActive;

namespace ToDoList.API.Endpoints;

public static class ItemEndpoints
{
    public static void MapItemEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/items")
            .WithTags("Items");

        group.MapPost("", CreateItem);
        group.MapGet("", GetAllItems);
        group.MapGet("/{id:guid}", GetItemById);
        group.MapPut("/{id:guid}", UpdateItem);
        group.MapDelete("/{id:guid}", DeleteItem);
        group.MapPut("/toggleActive/{id:guid}", ToggleActive);
        group.MapPut("/isCompleted/{id:guid}", ToggleIsCompleted);
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

    public static async Task<IResult> GetItemById(Guid id, IMediator mediator, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetItemByIdQuery(id), cancellationToken);
        return result is null
            ? Results.Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "Item não encontrado",
                detail: $"Não foi encontrado um item com o ID '{id}'.")
            : Results.Ok(result);
    }

    public static async Task<IResult> UpdateItem(Guid id, [FromBody] UpdateItemCommand command, IMediator mediator, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(command with { Id = id }, cancellationToken);
        return result is null
            ? Results.Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "Item não encontrado",
                detail: $"Não foi possível atualizar o item com o ID '{id}'.")
            : Results.Ok(result);
    }

    public static async Task DeleteItem(Guid id, IMediator mediator, CancellationToken cancellationToken)
     => await mediator.Send(new DeleteItemCommand(id), cancellationToken);

    public static async Task<IResult> ToggleActive(Guid id, [FromBody] UpdateToggleActiveCommand command, IMediator mediator, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(command with { Id = id }, cancellationToken);
        return result is null
            ? Results.Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "Item não encontrado",
                detail: $"Não foi possível atualizar o item com o ID '{id}'.")
            : Results.Ok(result);
    }

    public static async Task<IResult> ToggleIsCompleted(Guid id, [FromBody] UpdateIsCompletedCommand command, IMediator mediator, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(command with { Id = id }, cancellationToken);
        return result is null
            ? Results.Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "Item não encontrado",
                detail: $"Não foi possível atualizar o item com o ID '{id}'.")
            : Results.Ok(result);
    }
}
