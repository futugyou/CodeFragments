using System.Text.Json.Serialization;

namespace Mastercard.Model.v60
{
    public class RetrieveOrderResponse : BaseResponse
    {
        [JsonPropertyName("status")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public RetrieveStatus Status { get; set; }
    }
    public enum RetrieveStatus
    {
        AUTHENTICATED,
        AUTHENTICATION_INITIATED,
        AUTHENTICATION_NOT_NEEDED,
        AUTHENTICATION_UNSUCCESSFUL,
        AUTHORIZED,
        CANCELLED,
        CAPTURED,
        CHARGEBACK_PROCESSED,
        DISPUTED,
        EXCESSIVELY_REFUNDED,
        FAILED,
        FUNDING,
        INITIATED,
        PARTIALLY_CAPTURED,
        PARTIALLY_REFUNDED,
        REFUNDED,
        REFUND_REQUESTED,
        VERIFIED,
    }
}
