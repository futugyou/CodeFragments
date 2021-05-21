using Mastercard.Model;
using Mastercard.Model.v60;
using System.Threading.Tasks;

namespace Mastercard.Proxy
{
    public interface IMastercardProxy
    {
        Task<MerchantInfo> GetMerchantInfo();
        Task<PayResponse> Pay(PayRequest request);
        Task<CreateCheckoutSessionResponse> CreateCheckoutSession(CreateCheckoutSessionRequest request);
        Task<RetrieveOrderResponse> RetrieveOrder(RetrieveOrderRequest request);
        Task<RefundOrderResponse> RefundeOrder(RefundOrderRequest request);
    }
}
