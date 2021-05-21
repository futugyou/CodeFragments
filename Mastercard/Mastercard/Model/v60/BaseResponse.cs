using System.Text.Json.Serialization;

namespace Mastercard.Model.v60
{
    public class BaseResponse
    {
        [JsonPropertyName("error")]
        public Error Error { get; set; }
        [JsonPropertyName("result")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public ResponseResult Result { get; set; }
    }

    public class Error
    {
        [JsonPropertyName("cause")]
        public ErrorCause Cause { get; set; }
        [JsonPropertyName("explanation")]
        public string Explanation { get; set; }
        [JsonPropertyName("field")]
        public string Field { get; set; }
        [JsonPropertyName("supportCode")]
        public string SupportCode { get; set; }
        [JsonPropertyName("validationType")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public ErrorValidationType ValidationType { get; set; }
    }
    public enum ErrorValidationType
    {
        INVALID,
        MISSING,
        UNSUPPORTED
    }
    public enum ResponseResult
    {
        ERROR,
        FAILURE,
        PENDING,
        SUCCESS,
        UNKNOWN,
    }
    public enum ErrorCause
    {
        INVALID_REQUEST,
        REQUEST_REJECTED,
        SERVER_BUSY,
        SERVER_FAILED
    }
}
