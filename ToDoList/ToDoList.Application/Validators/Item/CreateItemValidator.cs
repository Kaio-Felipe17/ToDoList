using FluentValidation;
using ToDoList.Application.Items.Create;

namespace ToDoList.Application.Validators.Item;

public sealed class CreateItemValidator : AbstractValidator<CreateItemCommand>
{
    public CreateItemValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Nome é obrigatório.")
            .MaximumLength(100)
            .WithMessage("Nome deve ter no máximo 100 caracteres.");

        RuleFor(x => x.Description)
            .MaximumLength(200)
            .WithMessage("Descrição deve ter no máximo 200 caracteres.");
    }
}
