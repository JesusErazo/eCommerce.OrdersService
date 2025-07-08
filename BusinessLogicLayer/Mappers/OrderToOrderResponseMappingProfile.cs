using AutoMapper;
using eCommerce.OrdersService.BusinessLogicLayer.DTO;
using eCommerce.OrdersService.DataAccessLayer.Entities;

namespace eCommerce.OrdersService.BusinessLogicLayer.Mappers;

public class OrderToOrderResponseMappingProfile : Profile
{
  public OrderToOrderResponseMappingProfile()
  {
    CreateMap<Order, OrderResponse>();
  }
}
