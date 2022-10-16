using AutoMapper;
using Mango.Service.ShoppingCartApi.Models;
using Mango.Service.ShoppingCartApi.Models.Dto;

namespace Mango.Service.ShoppingCartApi;

public class MappingConfig
{
    public static MapperConfiguration RegisterMap()
    {
        var mapperConfig = new MapperConfiguration(config =>
        {
            config.CreateMap<ProductDto, Product>().ReverseMap();
            config.CreateMap<CartHeaderDto, CartHeader>().ReverseMap();
            config.CreateMap<CartDetailsDto, CartDetails>().ReverseMap();
            config.CreateMap<CartDto, Cart>().ReverseMap();
        });

        return mapperConfig;
    }
}