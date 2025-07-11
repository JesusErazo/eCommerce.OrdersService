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

  [HttpGet("search/orderid/{orderID:guid}",Name = "GetOrderByOrderID")]
  public async Task<OrderResponse?> GetOrderByOrderID(Guid orderID)
  {
    if (orderID == Guid.Empty) return null;

    FilterDefinition<Order> filter = Builders<Order>.Filter.Eq(order => order.OrderID, orderID);

    return await _ordersService.GetOrderByCondition(filter);
  }

  [HttpGet("search/productid/{productID:guid}")]
  public async Task<IEnumerable<OrderResponse?>> GetOrdersByProductID(Guid productID)
  {
    if (productID == Guid.Empty) return null;

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
    DateTime start = orderDate;
    DateTime end = orderDate.AddDays(1);

    FilterDefinition<Order> filter = 
      Builders<Order>.Filter.Gte(order => order.OrderDate, start) &
      Builders<Order>.Filter.Lt(order => order.OrderDate, end);

    return await _ordersService.GetOrdersByCondition(filter); 
  }

  [HttpGet("search/userid/{userID:guid}")]
  public async Task<IEnumerable<OrderResponse?>> GetOrdersByUserID(Guid userID)
  {
    if (userID == Guid.Empty) return null;

    FilterDefinition<Order> filter = Builders<Order>.Filter
      .Eq(order => order.UserID,userID);

    return await _ordersService.GetOrdersByCondition(filter);
  }

  [HttpPost]
  public async Task<IActionResult> AddOrder(OrderAddRequest orderAddRequest)
  {
    if (orderAddRequest is null) return BadRequest("Invalid order data");

    OrderResponse? createdOrder = await _ordersService.AddOrder(orderAddRequest);
    
    if (createdOrder is null) return Problem("Error in adding order");
    
    return CreatedAtRoute("GetOrderByOrderID", new { orderID = createdOrder.OrderID}, createdOrder);
  }

  [HttpPut("{orderID:guid}")]
  public async Task<IActionResult> UpdateOrder(Guid orderID, OrderUpdateRequest orderUpdateRequest)
  {
    if (orderID == Guid.Empty) return BadRequest("The orderID in the URL is invalid");

    if (orderUpdateRequest is null) return BadRequest("Invalid order data");

    if (orderID != orderUpdateRequest.OrderID) return BadRequest("orderID in the URL doesn't match with the orderID in the request body.");

    OrderResponse? updatedOrder = await _ordersService.UpdateOrder(orderUpdateRequest);

    if (updatedOrder is null) return Problem("Error in updating order");

    return Ok(updatedOrder);
  }

  [HttpDelete("{orderID:guid}")]
  public async Task<IActionResult> DeleteOrder(Guid orderID)
  {
    if (orderID == Guid.Empty) return BadRequest("Invalid orderID");

    bool isDeleted = await _ordersService.DeleteOrder(orderID);

    if (!isDeleted) return Problem("Error in deleting order");

    return Ok(isDeleted);
  }

}
