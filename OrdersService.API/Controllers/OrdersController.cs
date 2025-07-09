using eCommerce.OrdersService.BusinessLogicLayer.DTO;
using eCommerce.OrdersService.BusinessLogicLayer.ServiceContracts;
using eCommerce.OrdersService.DataAccessLayer.Entities;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace eCommerce.OrdersService.API.Controllers;

[Route("api/orders")]
[ApiController]
public class OrdersController : ControllerBase
{
  private readonly IOrdersService _ordersService;
  public OrdersController(IOrdersService ordersService)
  {
    _ordersService = ordersService;
  }

  [HttpGet]
  public async Task<IEnumerable<OrderResponse?>> GetOrders()
  {
    return await _ordersService.GetOrders();
  }

  [HttpGet("search/orderid/{orderID:guid}")]
  public async Task<OrderResponse?> GetOrderByOrderID(Guid orderID)
  {
    FilterDefinition<Order> filter = Builders<Order>.Filter.Eq(order => order.OrderID, orderID);

    return await _ordersService.GetOrderByCondition(filter);
  }

  [HttpGet("search/productid/{productID:guid}")]
  public async Task<IEnumerable<OrderResponse?>> GetOrdersByProductID(Guid productID)
  {
    FilterDefinition<Order> filter = Builders<Order>.Filter
      .ElemMatch(
        order => order.OrderItems, 
        Builders<OrderItem>.Filter.Eq(orderItem => orderItem.ProductID, productID)
      );

    return await _ordersService.GetOrdersByCondition(filter);
  }

  [HttpGet("search/orderdate/{orderDate:datetime}")]
  public async Task<IEnumerable<OrderResponse?>> GetOrdersByOrderDate(DateTime orderDate)
  {
    FilterDefinition<Order> filter = Builders<Order>.Filter
      .Eq(
        order => order.OrderDate.ToString("yyyy-MM-dd"), 
        orderDate.ToString("yyyy-MM-dd")
      );

    return await _ordersService.GetOrdersByCondition(filter); 
  }

}
