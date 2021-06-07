namespace Mastercard
{
    public class MastercardOptions
    {
        public string BaseUrl { get; private set; }
        public string Version { get; private set; } = "60";
        public string Currency { get; private set; } = "THB";
        public string MerchantId { get; private set; }
        public string Password { get; private set; }
        public string Username => "merchant." + MerchantId;
        public bool AuthenticationByCertificate { get; private set; } = false;
    }
}
