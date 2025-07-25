﻿namespace eCommerce.OrdersService.BusinessLogicLayer.DTO;

public record OrderAddRequest(
  Guid UserID,
  DateTime OrderDate,
  List<OrderItemAddRequest> OrderItems
  )
{
  public OrderAddRequest(): this(default, default, default) {}
};
