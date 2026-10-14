using ToDoList.Domain.Common;

namespace ToDoList.Domain.Entities;

public class Item : BaseEntity
{
    public required string Name { get; set; }
    public string? Description { get; set; }
    public bool IsCompleted { get; set; }

    public static Item Create(string name, string? description)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("O nome é obrigatório.", nameof(name));

        name = name.Trim();
        description = description?.Trim();

        if (name.Length > 100)
            throw new ArgumentException("O nome deve ter no máximo 100 caracteres.", nameof(name));
        if (description?.Length > 200)
            throw new ArgumentException("A descrição deve ter no máximo 200 caracteres.", nameof(description));

        return new Item { Name = name, Description = description, Active = true };
    }
}