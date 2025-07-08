using AutoMapper;
using eCommerce.OrdersService.BusinessLogicLayer.DTO;
using eCommerce.OrdersService.DataAccessLayer.Entities;

namespace eCommerce.OrdersService.BusinessLogicLayer.Mappers;

public class OrderUpdateRequestToOrderMappingProfile : Profile
{
  public OrderUpdateRequestToOrderMappingProfile()
  {
    CreateMap<OrderUpdateRequest, Order>()
      .ForMember(order => order._id, opt => opt.Ignore())
      .ForMember(order => order.TotalBill, opt => opt.Ignore());
  }
}
