using System.Text.Json.Serialization;

namespace Mastercard.Model.v60
{
    public class CreateCheckoutSessionResponse : BaseResponse
    {
        [JsonPropertyName("merchant")]
        public string Merchant { get; set; }
        [JsonPropertyName("session")]
        public Session Session { get; set; }
        [JsonPropertyName("successIndicator")]
        public string SuccessIndicator { get; set; }
    }

    public class Session
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }
        [JsonPropertyName("updateStatus")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public UpdateStatus UpdateStatus { get; set; }
        [JsonPropertyName("version")]
        public string Version { get; set; }
    }

    public enum UpdateStatus
    {
        FAILURE,
        NO_UPDATE,
        SUCCESS,
    }
}
