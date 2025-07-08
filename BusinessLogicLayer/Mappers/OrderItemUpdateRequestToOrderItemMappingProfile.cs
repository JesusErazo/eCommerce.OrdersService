using AutoMapper;
using eCommerce.OrdersService.BusinessLogicLayer.DTO;
using eCommerce.OrdersService.DataAccessLayer.Entities;
using MongoDB.Driver;

namespace eCommerce.OrdersService.BusinessLogicLayer.Mappers;

public class OrderItemUpdateRequestToOrderItemMappingProfile : Profile
{
  public OrderItemUpdateRequestToOrderItemMappingProfile()
  {
    CreateMap<OrderItemUpdateRequest, OrderItem>()
      .ForMember(orderItem => orderItem._id, opt => opt.Ignore())
      .ForMember(orderItem => orderItem.TotalPrice, opt => opt.Ignore());
  }
}
