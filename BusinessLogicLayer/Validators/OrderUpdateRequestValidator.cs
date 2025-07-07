using eCommerce.OrdersService.BusinessLogicLayer.DTO;
using FluentValidation;

namespace eCommerce.OrdersService.BusinessLogicLayer.Validators;

public class OrderUpdateRequestValidator : AbstractValidator<OrderUpdateRequest>
{
  public OrderUpdateRequestValidator()
  {
    //OrderID
    RuleFor(order => order.OrderID)
      .NotEmpty().WithMessage("Order ID can't be blank");

    //UserID
    RuleFor(order => order.UserID)
      .NotEmpty().WithMessage("User ID can't be blank");

    //OrderDate
    RuleFor(order => order.OrderDate)
      .NotEmpty().WithMessage("Order Date can't be blank");

    //OrderItems
    RuleFor(order => order.OrderItems)
      .NotEmpty().WithMessage("Order Items can't be blank");
  }
}
