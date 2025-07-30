using AutoMapper;
using eCommerce.OrdersService.BusinessLogicLayer.DTO;
using eCommerce.OrdersService.BusinessLogicLayer.Extensions;
using eCommerce.OrdersService.BusinessLogicLayer.HttpClients;
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
  private UsersMicroserviceClient _usersMicroserviceClient;
  private ProductsMicroserviceClient _productsMicroserviceClient;


  public OrdersService(
    IOrdersRepository ordersRepository, 
    IMapper mapper,
    IValidator<OrderAddRequest> orderAddValidator,
    IValidator<OrderItemAddRequest> orderItemAddValidator,
    IValidator<OrderUpdateRequest> orderUpdateValidator,
    IValidator<OrderItemUpdateRequest> orderItemUpdateValidator,
    UsersMicroserviceClient usersMicroserviceClient,
    ProductsMicroserviceClient productsMicroserviceClient
    )
  {
    _ordersRepository = ordersRepository;
    _mapper = mapper;
    _orderAddValidator = orderAddValidator;
    _orderItemAddValidator = orderItemAddValidator;
    _orderUpdateValidator = orderUpdateValidator;
    _orderItemUpdateValidator = orderItemUpdateValidator;
    _usersMicroserviceClient = usersMicroserviceClient;
    _productsMicroserviceClient = productsMicroserviceClient;
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
    
    //Validate if UserID exists in Users microservice
    UserResponse? existingUser = await _usersMicroserviceClient.GetUserByUserID(orderAddRequest.UserID);

    if (existingUser is null)
    {
      throw new ArgumentException("Invalid User ID");
    }

    //Logic for checking if productID exist in products microservice
    Guid[] productIDs = orderAddRequest.OrderItems
      .Select(x => x.ProductID)
      .Distinct()
      .ToArray();

    IEnumerable<ProductResponse?> existingProducts = await ValidateAndGetProductIDsExistInProductsMS(productIDs);

    Order orderInput = _mapper.Map<Order>(orderAddRequest);

    //Generate values
    CalculateOrderValues(orderInput);

    Order? addedOrder = await _ordersRepository.AddOrder(orderInput);

    if (addedOrder is null) return null;

    OrderResponse addedOrderResponse = _mapper.Map<OrderResponse>(addedOrder);

    //Set order user details
    SetOrderUserDetails(addedOrderResponse, existingUser);

    //Set order item details
    SetOrderItemsDetails(addedOrderResponse, existingProducts);

    return addedOrderResponse;
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

    OrderResponse orderResponse = _mapper.Map<OrderResponse>(matchedOrder);

    UserResponse? userData = await _usersMicroserviceClient.GetUserByUserID(orderResponse.UserID);
    
    if(userData != null)
    {
      SetOrderUserDetails(orderResponse, userData);
    }

    await GetAndSetOrderItemsDetails(orderResponse);

    return orderResponse;
  }

  public async Task<List<OrderResponse?>> GetOrders()
  {
    IEnumerable<Order> orders = await _ordersRepository.GetOrders();

    List<OrderResponse?> orderResponses = _mapper.Map<List<OrderResponse?>>(orders);

    await GetAndSetOrdersUserDetails(orderResponses);

    foreach (OrderResponse? orderResponse in orderResponses)
    {
      if (orderResponse is null) continue;
      await GetAndSetOrderItemsDetails(orderResponse);
    }

    return orderResponses;
  }

  public async Task<List<OrderResponse?>> GetOrdersByCondition(FilterDefinition<Order> filter)
  {
    IEnumerable<Order?> matchedOrders = await _ordersRepository.GetOrdersByCondition(filter);
    List<OrderResponse?> orderResponses = _mapper.Map<List<OrderResponse?>>(matchedOrders);

    await GetAndSetOrdersUserDetails(orderResponses);

    foreach (OrderResponse? orderResponse in orderResponses)
    {
      if (orderResponse is null) continue;
      await GetAndSetOrderItemsDetails(orderResponse);
    }
    
    return orderResponses;
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

    //Validate if UserID exists in Users microservice
    UserResponse? existingUser = await _usersMicroserviceClient.GetUserByUserID(orderUpdateRequest.UserID);

    if (existingUser is null)
    {
      throw new ArgumentException("Invalid User ID");
    }

    //logic for checking if product IDs exist in products microservice
    Guid[] productIDs = orderUpdateRequest.OrderItems
      .Select(x => x.ProductID)
      .Distinct()
      .ToArray();

    IEnumerable<ProductResponse?> existingProducts = await ValidateAndGetProductIDsExistInProductsMS(productIDs);

    Order orderInput = _mapper.Map<Order>(orderUpdateRequest);

    //Generate values
    CalculateOrderValues(orderInput);

    Order? updatedOrder = await _ordersRepository.UpdateOrder(orderInput);

    if (updatedOrder is null) return null;

    OrderResponse updatedOrderResponse = _mapper.Map<OrderResponse>(updatedOrder);

    //Set order user details
    SetOrderUserDetails(updatedOrderResponse, existingUser);

    //Set order item details
    SetOrderItemsDetails(updatedOrderResponse, existingProducts);

    return updatedOrderResponse;
  }

  private async Task GetAndSetOrdersUserDetails(IEnumerable<OrderResponse?> orders)
  {
    if(orders is null || !orders.Any()) return;

    Guid[] userIDs = orders
      .Where(order => order is not null && order.UserID != Guid.Empty)
      .Select(order => order!.UserID)
      .Distinct()
      .ToArray();

    if (userIDs.Length < 1) return;

    IEnumerable<UserResponse?> usersData = await _usersMicroserviceClient.GetUsersByUserIDs(userIDs);
    
    if (!usersData.Any()) return;

    Dictionary<Guid, UserResponse?> usersDataDict = usersData
      .Where(u => u != null)
      .ToDictionary(u => u!.UserID);

    foreach(OrderResponse? order in orders)
    {
      if(order is null) continue;

      if (usersDataDict.TryGetValue(order.UserID, out UserResponse? userData))
      {
        SetOrderUserDetails(order, userData);
      }
    }
  }

  private void SetOrderUserDetails(OrderResponse? order, UserResponse? userData)
  {
    if (order is null || userData is null) return;

    if (userData.UserID != Guid.Empty)
    {
      if (order.UserID != userData.UserID) return;
    }

    _mapper.Map(userData, order);
  }

  private async Task GetAndSetOrderItemsDetails(OrderResponse? order)
  {
    //Get and Set details of each order item from products microservice
    
    if (order is null || order.OrderItems.Count < 1) return;

    Guid[] orderProductIDs = order.OrderItems
      .Select(item => item.ProductID)
      .Distinct()
      .ToArray();

    IEnumerable<ProductResponse?> orderProducts = await _productsMicroserviceClient.GetProductsByProductIDs(orderProductIDs);

    SetOrderItemsDetails(order, orderProducts);
  }

  private void SetOrderItemsDetails(OrderResponse? order, IEnumerable<ProductResponse?> orderProductsData)
  {
    //Set details of each order item from provided orderProductsData object

    if (order is null || order.OrderItems.Count < 1) return;

    if (orderProductsData.Count() < 1) return;

    Dictionary<Guid, ProductResponse?> orderProductsDict = orderProductsData
      .Where(p => p != null)
      .ToDictionary(p => p!.ProductID);

    foreach (OrderItemResponse item in order.OrderItems)
    {
      if (orderProductsDict.TryGetValue(item.ProductID, out ProductResponse? product))
      {
        _mapper.Map(product, item);
      }
    }
  }

  private async Task<IEnumerable<ProductResponse?>> ValidateAndGetProductIDsExistInProductsMS(Guid[] productIDs)
  {
    IEnumerable<ProductResponse?> existingProducts = await _productsMicroserviceClient.GetProductsByProductIDs(productIDs);

    if (existingProducts.Count() != productIDs.Length)
    {
      Guid[] missingProductIDs = productIDs
        .Where(productID => !existingProducts.Any(product => product != null && product.ProductID == productID))
        .Distinct()
        .ToArray();

      throw new ArgumentException($"Invalid Product IDs: {string.Join(", ", missingProductIDs)}");
    }

    return existingProducts;
  }

  private void CalculateOrderValues(Order order)
  {
    foreach (OrderItem item in order.OrderItems)
    {
      item.TotalPrice = (item.UnitPrice * item.Quantity);
    }

    order.TotalBill = order.OrderItems.Sum(item => item.TotalPrice);
  }
}
