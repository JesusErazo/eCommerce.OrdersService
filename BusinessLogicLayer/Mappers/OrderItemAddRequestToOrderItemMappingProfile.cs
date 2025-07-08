using AutoMapper;
using eCommerce.OrdersService.BusinessLogicLayer.DTO;
using eCommerce.OrdersService.DataAccessLayer.Entities;

namespace eCommerce.OrdersService.BusinessLogicLayer.Mappers;

public class OrderItemAddRequestToOrderItemMappingProfile : Profile
{
  public OrderItemAddRequestToOrderItemMappingProfile() {
    CreateMap<OrderItemAddRequest, OrderItem>()
      .ForMember(orderItem => orderItem._id, opt => opt.Ignore())
      .ForMember(orderItem => orderItem.TotalPrice, opt => opt.Ignore());
  }
}
