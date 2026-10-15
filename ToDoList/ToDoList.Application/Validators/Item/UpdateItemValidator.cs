using FluentValidation;
using ToDoList.Application.Items.Update;

namespace ToDoList.Application.Validators.Item;

public sealed class UpdateItemValidator : AbstractValidator<UpdateItemCommand>
{
    public UpdateItemValidator()
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
