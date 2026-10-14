using MediatR;
using ToDoList.Application.DTOs.Item;

namespace ToDoList.Application.Items.GetAll;

public class GetAllItemsQuery : IRequest<List<ItemDTO>>;
