using AutoMapper;
using eCommerce.OrdersService.BusinessLogicLayer.DTO;

namespace eCommerce.OrdersService.BusinessLogicLayer.Mappers;

public class ProductResponseToOrderItemResponseMappingProfile : Profile
{
  public ProductResponseToOrderItemResponseMappingProfile()
  {
    CreateMap<ProductResponse, OrderItemResponse>()
      .ForMember(dest => dest.ProductID, opt => opt.Ignore())
      .ForMember(dest => dest.UnitPrice, opt => opt.Ignore())
      .ForMember(dest => dest.Quantity, opt => opt.Ignore())
      .ForMember(dest => dest.TotalPrice, opt => opt.Ignore());
  }
}
