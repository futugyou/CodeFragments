using System.Text.Json.Serialization;

namespace Mastercard.Model.v60
{
    public class RefundOrderResponse : BaseResponse
    {
        [JsonPropertyName("response")]
        public RefundResponse Response { get; set; }
        [JsonPropertyName("transaction")]
        public TransactionResponse Transaction { get; set; } = new TransactionResponse();
    }
    public class RefundResponse
    {
        [JsonPropertyName("gatewayCode")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public GatewayCodeEnum GatewayCode { get; set; }
    }
    public class TransactionResponse
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }
        [JsonPropertyName("amount")]
        public decimal Amount { get; set; }
        [JsonPropertyName("currency")]
        public string Currency { get; set; }
        [JsonPropertyName("type")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public TransactionTypeEnum Type { get; set; }
    }
    public enum GatewayCodeEnum
    {
        ABORTED,
        ACQUIRER_SYSTEM_ERROR,
        APPROVED,
        APPROVED_AUTO,
        APPROVED_PENDING_SETTLEMENT,
        AUTHENTICATION_FAILED,
        AUTHENTICATION_IN_PROGRESS,
        BALANCE_AVAILABLE,
        BALANCE_UNKNOWN,
        BLOCKED,
        CANCELLED,
        DECLINED,
        DECLINED_AVS,
        DECLINED_AVS_CSC,
        DECLINED_CSC,
        DECLINED_DO_NOT_CONTACT,
        DECLINED_INVALID_PIN,
        DECLINED_PAYMENT_PLAN,
        DECLINED_PIN_REQUIRED,
        DEFERRED_TRANSACTION_RECEIVED,
        DUPLICATE_BATCH,
        EXCEEDED_RETRY_LIMIT,
        EXPIRED_CARD,
        INSUFFICIENT_FUNDS,
        INVALID_CSC,
        LOCK_FAILURE,
        NOT_ENROLLED_3D_SECURE,
        NOT_SUPPORTED,
        NO_BALANCE,
        PARTIALLY_APPROVED,
        PENDING,
        REFERRED,
        SUBMITTED,
        SYSTEM_ERROR,
        TIMED_OUT,
        UNKNOWN,
        UNSPECIFIED_FAILURE,
    }
    public enum TransactionTypeEnum
    {
        AUTHENTICATION,
        AUTHORIZATION,
        AUTHORIZATION_UPDATE,
        CAPTURE,
        CHARGEBACK,
        FUNDING,
        PAYMENT,
        REFUND,
        REFUND_REQUEST,
        VERIFICATION,
        VOID_AUTHORIZATION,
        VOID_CAPTURE,
        VOID_PAYMENT,
        VOID_REFUND,
    }
}
