namespace eCommerce.OrdersService.BusinessLogicLayer.DTO;

public record ValidationFailureResponse(
  List<ValidationFailureItem> errors
  );
