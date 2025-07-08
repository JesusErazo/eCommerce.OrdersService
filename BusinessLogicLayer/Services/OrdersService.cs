using AutoMapper;
using eCommerce.OrdersService.BusinessLogicLayer.DTO;
using eCommerce.OrdersService.BusinessLogicLayer.Extensions;
using eCommerce.OrdersService.BusinessLogicLayer.ServiceContracts;
using eCommerce.OrdersService.DataAccessLayer.Entities;
using eCommerce.OrdersService.DataAccessLayer.RepositoryContracts;
using FluentValidation;
using FluentValidation.Results;
using MongoDB.Driver;

namespace eCommerce.OrdersService.BusinessLogicLayer.Services;

public class OrdersService : IOrdersService
{
  private readonly IOrdersRepository _ordersRepository;
  private readonly IMapper _mapper;
  private readonly IValidator<OrderAddRequest> _orderAddValidator;
  private readonly IValidator<OrderItemAddRequest> _orderItemAddValidator;
  private readonly IValidator<OrderUpdateRequest> _orderUpdateValidator;
  private readonly IValidator<OrderItemUpdateRequest> _orderItemUpdateValidator;


  public OrdersService(
    IOrdersRepository ordersRepository, 
    IMapper mapper,
    IValidator<OrderAddRequest> orderAddValidator,
    IValidator<OrderItemAddRequest> orderItemAddValidator,
    IValidator<OrderUpdateRequest> orderUpdateValidator,
    IValidator<OrderItemUpdateRequest> orderItemUpdateValidator
    )
  {
    _ordersRepository = ordersRepository;
    _mapper = mapper;
    _orderAddValidator = orderAddValidator;
    _orderItemAddValidator = orderItemAddValidator;
    _orderUpdateValidator = orderUpdateValidator;
    _orderItemUpdateValidator = orderItemUpdateValidator;
  }

  public async Task<OrderResponse?> AddOrder(OrderAddRequest orderAddRequest)
  {
    ArgumentNullException.ThrowIfNull(orderAddRequest);

    ValidationResult orderValidationResult = await _orderAddValidator.ValidateAsync( orderAddRequest );

    if (!orderValidationResult.IsValid)
    {
      string errors = orderValidationResult.Errors.ToBasicFormat().ToString();
      throw new ArgumentException(errors);
    }

    //Validate each order item using Fluent Validation
    foreach (OrderItemAddRequest item in orderAddRequest.OrderItems)
    {
      ValidationResult orderItemValidationResult = await _orderItemAddValidator.ValidateAsync(item);

      if (!orderItemValidationResult.IsValid) {
        string errors = orderItemValidationResult.Errors.ToBasicFormat().ToString();
        throw new ArgumentException(errors);
      }
    }

    //TO DO: Add logic for checking if UserID exists in Users microservice

    Order orderInput = _mapper.Map<Order>(orderAddRequest);

    //Generate values
    foreach (OrderItem item in orderInput.OrderItems)
    {
      item.TotalPrice = (item.UnitPrice * item.Quantity);
    }

    orderInput.TotalBill = orderInput.OrderItems.Sum(item => item.TotalPrice);

    Order? addedOrder = await _ordersRepository.AddOrder(orderInput);

    if (addedOrder is null) return null;

    return _mapper.Map<OrderResponse>(addedOrder);
  }

  public async Task<bool> DeleteOrder(Guid orderID)
  {
    FilterDefinition<Order> filter = Builders<Order>.Filter.Eq(order => order.OrderID, orderID);
    Order? existingOrder = await _ordersRepository.GetOrderByCondition(filter);

    if (existingOrder is null) return false;

    bool isDeleted = await _ordersRepository.DeleteOrder(orderID);

    return isDeleted;
  }

  public async Task<OrderResponse?> GetOrderByCondition(FilterDefinition<Order> filter)
  {
    Order? matchedOrder = await _ordersRepository.GetOrderByCondition(filter);

    if(matchedOrder is null) return null;

    return _mapper.Map<OrderResponse>(matchedOrder);
  }

  public async Task<List<OrderResponse?>> GetOrders()
  {
    IEnumerable<Order> orders = await _ordersRepository.GetOrders();
    return _mapper.Map<List<OrderResponse?>>(orders);
  }

  public async Task<List<OrderResponse?>> GetOrdersByCondition(FilterDefinition<Order> filter)
  {
    IEnumerable<Order?> matchedOrders = await _ordersRepository.GetOrdersByCondition(filter);    
    return _mapper.Map<List<OrderResponse?>>(matchedOrders);
  }

  public async Task<OrderResponse?> UpdateOrder(OrderUpdateRequest orderUpdateRequest)
  {
    ArgumentNullException.ThrowIfNull(orderUpdateRequest);

    ValidationResult orderUpdateValidationResult = await _orderUpdateValidator.ValidateAsync(orderUpdateRequest);

    if (!orderUpdateValidationResult.IsValid)
    {
      string errors = orderUpdateValidationResult.Errors.ToBasicFormat().ToString();
      throw new ArgumentException(errors);
    }

    //Validate each order item using Fluent Validation
    foreach (OrderItemUpdateRequest item in orderUpdateRequest.OrderItems)
    {
      ValidationResult orderItemUpdateValidationResult = await _orderItemUpdateValidator.ValidateAsync(item);

      if (!orderItemUpdateValidationResult.IsValid)
      {
        string errors = orderItemUpdateValidationResult.Errors.ToBasicFormat().ToString();
        throw new ArgumentException(errors);
      }
    }

    //TO DO: Add logic for checking if UserID exists in Users microservice

    Order orderInput = _mapper.Map<Order>(orderUpdateRequest);

    //Generate values
    foreach (OrderItem item in orderInput.OrderItems)
    {
      item.TotalPrice = (item.UnitPrice * item.Quantity);
    }

    orderInput.TotalBill = orderInput.OrderItems.Sum(item => item.TotalPrice);

    Order? updatedOrder = await _ordersRepository.UpdateOrder(orderInput);

    if (updatedOrder is null) return null;

    return _mapper.Map<OrderResponse>(updatedOrder);
  }
}
