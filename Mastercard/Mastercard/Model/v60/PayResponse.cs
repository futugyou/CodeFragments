using System;
using System.Text.Json.Serialization;

namespace Mastercard.Model.v60
{
    public class PayResponse : BaseResponse
    {
        [JsonPropertyName("browserPayment")]
        public BrowserPayment BrowserPayment { get; set; }
        [JsonPropertyName("merchant")]
        public string Merchant { get; set; }
        [JsonPropertyName("order")]
        public PayOrderResponse Order { get; set; }
        [JsonPropertyName("gatewayCode")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public GatewayCodeEnum GatewayCode { get; set; }
        [JsonPropertyName("transaction")]
        public TransactionResponse Transaction { get; set; }
    }

    public class PayOrderResponse
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }
        [JsonPropertyName("currency")]
        public string Currency { get; set; }
        [JsonPropertyName("amount")]
        public decimal Amount { get; set; }
        [JsonPropertyName("creationTime")]
        public DateTime CreationTime { get; set; }
        [JsonPropertyName("lastUpdatedTime")]
        public DateTime LastUpdatedTime { get; set; }
        [JsonPropertyName("merchantAmount")]
        public decimal MerchantAmount { get; set; }
        [JsonPropertyName("merchantCurrency")]
        public string MerchantCurrency { get; set; }
        [JsonPropertyName("totalAuthorizedAmount")]
        public decimal TotalAuthorizedAmount { get; set; }
        [JsonPropertyName("totalCapturedAmount")]
        public decimal TotalCapturedAmount { get; set; }
        [JsonPropertyName("totalRefundedAmount")]
        public decimal TotalRefundedAmount { get; set; }
    }

    public class BrowserPayment
    {
        [JsonPropertyName("redirectUrl")]
        public string RedirectUrl { get; set; }
    }
}
