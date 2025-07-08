using eCommerce.OrdersService.BusinessLogicLayer.Mappers;
using eCommerce.OrdersService.BusinessLogicLayer.ServiceContracts;
using eCommerce.OrdersService.BusinessLogicLayer.Services;
using eCommerce.OrdersService.BusinessLogicLayer.Validators;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace eCommerce.OrdersService.BusinessLogicLayer;

public static class DependencyInjection
{
  public static IServiceCollection AddBusinessLogicLayer(this IServiceCollection services, IConfiguration configuration)
  {
    //TO DO: Add Business Logic Layer Services into IoC container.
    services.AddValidatorsFromAssemblyContaining<OrderAddRequestValidator>();

    //AutoMapper
    services.AddAutoMapper(typeof(OrderAddRequestToOrderMappingProfile).Assembly);

    services.AddScoped<IOrdersService, eCommerce.OrdersService.BusinessLogicLayer.Services.OrdersService>();
    return services;
  }

}
