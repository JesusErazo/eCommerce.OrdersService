using AutoMapper;
using eCommerce.OrdersService.BusinessLogicLayer.DTO;

namespace eCommerce.OrdersService.BusinessLogicLayer.Mappers;

public class UserResponseToOrderResponseMappingProfile : Profile
{
  public UserResponseToOrderResponseMappingProfile()
  {
    CreateMap<UserResponse, OrderResponse>()
      .ForMember(dest => dest.UserPersonName, opt => opt.MapFrom(src => src.PersonName))
      .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
      .ForMember(dest => dest.OrderID, opt => opt.Ignore())
      .ForMember(dest => dest.UserID, opt => opt.Ignore())
      .ForMember(dest => dest.TotalBill, opt => opt.Ignore())
      .ForMember(dest => dest.OrderDate, opt => opt.Ignore())
      .ForMember(dest => dest.OrderItems, opt => opt.Ignore());
  }
}
