using System.Text.Json.Serialization;

namespace Mastercard.Model.v60
{
    public class RefundOrderRequest
    {
        public string OrderId { get; set; }
        public string TransactionId { get; set; }
        public Refundobject Refundobject { get; set; }
    }
    public class Refundobject
    {
        [JsonPropertyName("apiOperation")]
        public string ApiOperation { get; set; } = "REFUND";
        [JsonPropertyName("transaction")]
        public Transaction Transaction { get; set; }
    }

    public class Transaction
    {
        [JsonPropertyName("amount")]
        public decimal Amount { get; set; }
        [JsonPropertyName("currency")]
        public string Currency { get; set; } = "THB";
    }

}
