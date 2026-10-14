using AutoMapper;
using ToDoList.Application.DTOs.Item;
using ToDoList.Domain.Entities;

namespace ToDoList.Application.Mappings;

public class ItemMappingProfile : Profile
{
    public ItemMappingProfile()
    {
        CreateMap<Item, ItemDTO>();
    }
}
