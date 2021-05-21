using System.Text.Json.Serialization;

namespace Mastercard.Model.v60
{
    public class PayRequest
    {
        public string OrderId { get; set; }
        public string TransactionId { get; set; }
        public Payobject Payobject { get; set; }
    }
    public class Payobject
    {
        [JsonPropertyName("apiOperation")]
        public string ApiOperation { get; set; } = "PAY";
        [JsonPropertyName("order")]
        public Order Order { get; set; }
        [JsonPropertyName("sourceOfFunds")]
        public SourceOfFunds SourceOfFunds { get; set; }
    }

    public class SourceOfFunds
    {
        [JsonPropertyName("token")]
        public string Token { get; set; }
        [JsonPropertyName("tokenRequestorID")]
        public string TokenRequestorID { get; set; }
    }
}
