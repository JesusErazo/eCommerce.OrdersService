using eCommerce.OrdersService.BusinessLogicLayer.DTO;
using FluentValidation;

namespace eCommerce.OrdersService.BusinessLogicLayer.Validators;

public class OrderItemUpdateRequestValidator : AbstractValidator<OrderItemUpdateRequest>
{
  public OrderItemUpdateRequestValidator()
  {
    //ProductID
    RuleFor(order => order.ProductID)
      .NotEmpty().WithMessage("Product ID can't be blank");
    
    //UnitPrice
    RuleFor(order => order.UnitPrice)
      .NotEmpty().WithMessage("Unit Price can't be blank")
      .GreaterThan(0).WithMessage("Unit Price can't be less than or equal to zero");

    //Quantity
    RuleFor(order => order.Quantity)
      .NotEmpty().WithMessage("Quantity can't be blank")
      .GreaterThan(0).WithMessage("Quantity can't be less than or equal to zero");
  }
}
