namespace ToDoList.Application.Exceptions;

public sealed class ItemNameConflictException : Exception
{
    public ItemNameConflictException() : base("Já existe um item cadastrado com esse nome.") { }
}