using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Mastercard.Model.v60
{
    public class CreateCheckoutSessionRequest
    {
        [JsonPropertyName("apiOperation")]
        public string ApiOperation { get; } = "CREATE_CHECKOUT_SESSION";
        [JsonPropertyName("order")]
        public Order Order { get; set; } = new Order();
        [JsonPropertyName("interaction")]
        public Interaction Interaction { get; set; } = new Interaction();
    }

    public class Order
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }
        [JsonPropertyName("currency")]
        public string Currency { get; set; } = "THB";
        [JsonPropertyName("amount")]
        public decimal Amount { get; set; }
    }

    public class Interaction
    {
        [JsonPropertyName("operation")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public InteractionOpertion Operation { get; set; } = InteractionOpertion.VERIFY;
    }
    public enum InteractionOpertion
    {
        AUTHORIZE,
        NONE,
        PURCHASE,
        VERIFY
    }
}
