using GrpcServer.Model;

namespace GrpcServer.Services;
public class OrderService : IOrderService
{
    private readonly ILogger<OrderService> _logger;
    public OrderService(ILogger<OrderService> logger)
    {
        _logger = logger;
    }
    public Task<OrderResponse> GetOrders(OrderRequest request)
    {
        _logger.LogInformation("order id is " + request.OrderId);
        var response = new OrderResponse
        {
            OrderId = request.OrderId,
            OrderTime = DateTime.Now,
            Amount = 923,
        };
        return Task.FromResult(response);
    }
}