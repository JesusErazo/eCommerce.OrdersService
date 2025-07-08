using AutoMapper;
using eCommerce.OrdersService.BusinessLogicLayer.DTO;
using eCommerce.OrdersService.DataAccessLayer.Entities;

namespace eCommerce.OrdersService.BusinessLogicLayer.Mappers;

public class OrderItemToOrderItemResponseMappingProfile : Profile
{
  public OrderItemToOrderItemResponseMappingProfile()
  {
    CreateMap<OrderItem, OrderItemResponse>();
  }
}
