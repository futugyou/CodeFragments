namespace Mastercard
{
    public class MastercardOptions
    {
        public string BaseUrl { get; set; }
        public string Version { get; set; } = "60";
        public string Currency { get; set; } = "THB";
        public string MerchantId { get; set; }
        public string Password { get; set; }
        public string Username => "merchant." + MerchantId;
        public bool AuthenticationByCertificate { get; set; } = false;
    }
}
