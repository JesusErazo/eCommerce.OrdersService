using eCommerce.OrdersService.DataAccessLayer.Entities;
using eCommerce.OrdersService.DataAccessLayer.RepositoryContracts;
using MongoDB.Driver;

namespace eCommerce.OrdersService.DataAccessLayer.Repositories;

public class OrdersRepository : IOrdersRepository
{
  private readonly IMongoCollection<Order> _ordersCollection;
  private readonly string collectionName = "orders";
  public OrdersRepository(IMongoDatabase mongoDatabase)
  {
     _ordersCollection = mongoDatabase.GetCollection<Order>(collectionName);
  }
  public async Task<Order?> AddOrder(Order order)
  {
    order.OrderID = Guid.NewGuid();
    order._id = order.OrderID;

    foreach (OrderItem orderItem in order.OrderItems)
    {
      orderItem._id = Guid.NewGuid();
    }

    await _ordersCollection.InsertOneAsync(order);
    return order;
  }

  public async Task<bool> DeleteOrder(Guid orderID)
  {
    FilterDefinition<Order> filter = Builders<Order>
      .Filter.Eq(orderDb => orderDb.OrderID, orderID);

    Order? existingOrder = (await _ordersCollection.FindAsync(filter)).FirstOrDefault();

    if (existingOrder is null) return false;

    DeleteResult deleteResult = await _ordersCollection.DeleteOneAsync(filter);

    return deleteResult.DeletedCount > 0;
  }

  public async Task<IEnumerable<Order>> GetOrders()
  {
    return (await _ordersCollection.FindAsync(Builders<Order>.Filter.Empty)).ToList();
  }

  public async Task<Order?> GetOrderByCondition(FilterDefinition<Order> filter)
  {
    return (await _ordersCollection.FindAsync(filter)).FirstOrDefault();
  }

  public async Task<IEnumerable<Order?>> GetOrdersByCondition(FilterDefinition<Order> filter)
  {
    return (await _ordersCollection.FindAsync(filter)).ToList();
  }

  public async Task<Order?> UpdateOrder(Order order)
  {
    if (order is null) return null;

    FilterDefinition<Order> filter = Builders<Order>.Filter.Eq(orderDb => orderDb.OrderID, order.OrderID);

    Order? existingOrder = (await _ordersCollection.FindAsync(filter)).FirstOrDefault();

    if (existingOrder is null) return null;

    order._id = existingOrder._id;

    ReplaceOneResult replaceResult = await _ordersCollection.ReplaceOneAsync(filter, order);

    if (replaceResult.IsModifiedCountAvailable)
    {
      if (replaceResult.ModifiedCount > 0)
      {
        return order;
      }
    }

    return null;
  }
}
