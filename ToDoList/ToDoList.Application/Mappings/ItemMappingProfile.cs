using AutoMapper;
using ToDoList.Application.DTOs.Item;
using ToDoList.Application.Items.Update;
using ToDoList.Domain.Entities;

namespace ToDoList.Application.Mappings;

public class ItemMappingProfile : Profile
{
    public ItemMappingProfile()
    {
        CreateMap<Item, ItemDTO>();
        CreateMap<UpdateItemCommand, Item>()
            .ForMember(dest => dest.Id, opt => opt.Ignore());
    }
}
