using System.Runtime.Serialization;
using System.ServiceModel;

namespace GrpcServer.Model;

[DataContract]
public class OrderRequest
{
    [DataMember(Order = 1)]
    public int OrderId { get; set; }
}
[DataContract]
public class OrderResponse
{
    [DataMember(Order = 1)]
    public int OrderId { get; set; }
    [DataMember(Order = 2)]
    public DateTime OrderTime { get; set; }
    [DataMember(Order = 3)]
    public double Amount { get; set; }
}

[ServiceContract]
public interface IOrderService
{
    [OperationContract]
    Task<OrderResponse> GetOrders(OrderRequest request);
}
