using FluentValidation;
using ToDoList.Application.Behaviors;
using ToDoList.Application.DTOs.Item;
using ToDoList.Application.Items.Create;
using ToDoList.Application.Items.Update;
using ToDoList.Application.Validators.Item;

namespace ToDoList.Tests;

public sealed class ValidationTests
{
    [Theory]
    [InlineData(null, null, false)]
    [InlineData("", null, false)]
    [InlineData("   ", null, false)]
    [InlineData("Task", null, true)]
    [InlineData("Task", "Details", true)]
    public async Task CreateAndUpdateApplySameRequiredNameRules(string? name, string? description, bool valid)
    {
        var create = await new CreateItemValidator().ValidateAsync(
            new CreateItemCommand { Name = name!, Description = description });
        var update = await new UpdateItemValidator().ValidateAsync(
            new UpdateItemCommand(Guid.NewGuid()) { Name = name!, Description = description });
        Assert.Equal(valid, create.IsValid);
        Assert.Equal(valid, update.IsValid);
        if (!valid)
        {
            Assert.Contains(create.Errors, error => error.PropertyName == "Name");
            Assert.Contains(update.Errors, error => error.PropertyName == "Name");
        }
    }

    [Theory]
    [InlineData(100, 200, true)]
    [InlineData(101, 200, false)]
    [InlineData(100, 201, false)]
    public async Task CreateAndUpdateEnforceLengthBoundaries(int nameLength, int descriptionLength, bool valid)
    {
        var name = new string('n', nameLength);
        var description = new string('d', descriptionLength);
        Assert.Equal(valid, (await new CreateItemValidator().ValidateAsync(
            new CreateItemCommand { Name = name, Description = description })).IsValid);
        Assert.Equal(valid, (await new UpdateItemValidator().ValidateAsync(
            new UpdateItemCommand(Guid.NewGuid()) { Name = name, Description = description })).IsValid);
    }

    [Fact]
    public async Task InvalidRequestCollectsErrorsAndDoesNotCallHandler()
    {
        var called = false;
        var behavior = new ValidationBehavior<CreateItemCommand, ItemDTO>([new CreateItemValidator()]);
        var exception = await Assert.ThrowsAsync<ValidationException>(() => behavior.Handle(
            new CreateItemCommand { Name = " ", Description = new string('d', 201) },
            _ => { called = true; return Task.FromResult(new ItemDTO { Name = "Task" }); }, CancellationToken.None));
        Assert.False(called);
        Assert.Contains(exception.Errors, error => error.PropertyName == "Name");
        Assert.Contains(exception.Errors, error => error.PropertyName == "Description");
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task ValidRequestCallsHandlerOnceAndReturnsItsResult(bool useValidator)
    {
        var calls = 0;
        var expected = new ItemDTO { Name = "Task" };
        var behavior = new ValidationBehavior<CreateItemCommand, ItemDTO>(
            useValidator ? [new CreateItemValidator()] : []);
        var result = await behavior.Handle(new CreateItemCommand { Name = "Task" },
            _ => { calls++; return Task.FromResult(expected); }, CancellationToken.None);
        Assert.Equal(1, calls);
        Assert.Same(expected, result);
    }
}
