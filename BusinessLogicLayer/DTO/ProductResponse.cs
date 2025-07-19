namespace eCommerce.OrdersService.BusinessLogicLayer.DTO;

public record ProductResponse(
  Guid ProductID,
  string? ProductName,
  string? Category,
  double UnitPrice,
  int QuantityInStock
  );
