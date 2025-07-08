using eCommerce.OrdersService.BusinessLogicLayer.DTO;
using FluentValidation.Results;

namespace eCommerce.OrdersService.BusinessLogicLayer.Extensions
{
  public static class ValidationFailureExtensions
  {
    public static ValidationFailureResponse ToBasicFormat(this IEnumerable<ValidationFailure> failures)
    {
      List<ValidationFailureItem> errors = failures.Select(error => new ValidationFailureItem(error.PropertyName, error.ErrorMessage)).ToList();
      return new ValidationFailureResponse(errors);
    }
  }
}
