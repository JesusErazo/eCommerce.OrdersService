namespace eCommerce.OrdersService.BusinessLogicLayer.DTO;

public record ValidationFailureItem(
  string propertyName,
  string errorMessage
  );
