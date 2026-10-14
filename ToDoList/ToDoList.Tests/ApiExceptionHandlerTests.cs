using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using ToDoList.API.Errors;
using ToDoList.Application.Exceptions;

namespace ToDoList.Tests;

public sealed class ApiExceptionHandlerTests
{
    [Theory]
    [InlineData(400)]
    [InlineData(409)]
    [InlineData(500)]
    public async Task ErrorsProduceProblemDetailsWithExpectedStatus(int expectedStatus)
    {
        using var services = new ServiceCollection().AddLogging().AddProblemDetails().BuildServiceProvider();
        using var body = new MemoryStream();
        var context = new DefaultHttpContext { RequestServices = services };
        context.Response.Body = body;
        Exception exception = expectedStatus switch
        {
            400 => new ArgumentException("Nome inválido"),
            409 => new ItemNameConflictException(),
            _ => new Exception("Sensitive internal detail")
        };

        var handler = new ApiExceptionHandler(services.GetRequiredService<IProblemDetailsService>());
        Assert.True(await handler.TryHandleAsync(context, exception, CancellationToken.None));
        Assert.Equal(expectedStatus, context.Response.StatusCode);
        Assert.StartsWith("application/problem+json", context.Response.ContentType);
        body.Position = 0;
        using var json = await JsonDocument.ParseAsync(body);
        Assert.Equal(expectedStatus, json.RootElement.GetProperty("status").GetInt32());
        Assert.NotEmpty(json.RootElement.GetProperty("detail").GetString()!);
        if (expectedStatus == 500)
            Assert.DoesNotContain("Sensitive", json.RootElement.GetProperty("detail").GetString());
    }
}