using AutoMapper;
using eCommerce.OrdersService.BusinessLogicLayer.DTO;
using eCommerce.OrdersService.DataAccessLayer.Entities;

namespace eCommerce.OrdersService.BusinessLogicLayer.Mappers;

public class OrderAddRequestToOrderMappingProfile : Profile
{
  public OrderAddRequestToOrderMappingProfile()
  {
    CreateMap<OrderAddRequest, Order>()
      .ForMember(order => order._id, opt => opt.Ignore())
      .ForMember(order => order.OrderID, opt => opt.Ignore())
      .ForMember(order => order.TotalBill, opt => opt.Ignore());
  }
}
