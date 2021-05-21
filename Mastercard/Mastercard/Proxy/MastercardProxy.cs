using Mastercard.Model;
using Mastercard.Model.v60;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Mastercard.Proxy
{
    public class MastercardProxy : IMastercardProxy
    {
        private readonly IHttpClientFactory factory;
        private readonly MastercardOptions options;
        private readonly ILogger<MastercardProxy> logger;

        public MastercardProxy(IOptionsMonitor<MastercardOptions> optionsMonitor, IHttpClientFactory factory, ILogger<MastercardProxy> logger)
        {
            this.options = optionsMonitor.CurrentValue;
            this.factory = factory;
            this.logger = logger;
        }

        private HttpClient CreateHttpClient()
        {
            var httpClient = factory.CreateClient("mastercard");
            string credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes(options.Username + ":" + options.Password));
            httpClient.BaseAddress = new Uri(options.BaseUrl);
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);
            return httpClient;
        }

        public async Task<CreateCheckoutSessionResponse> CreateCheckoutSession(CreateCheckoutSessionRequest request)
        {
            var httpclient = CreateHttpClient();
            var content = new StringContent(JsonSerializer.Serialize(request));
            var responseMessage = await httpclient.PostAsync($"api/rest/version/{options.Version}/merchant/{options.MerchantId}/session", content);
            var stream = await responseMessage.Content.ReadAsStreamAsync();
            return await JsonSerializer.DeserializeAsync<CreateCheckoutSessionResponse>(stream);
        }

        public Task<MerchantInfo> GetMerchantInfo()
        {
            MerchantInfo merchantInfo = new MerchantInfo
            {
                BaseUrl = options.BaseUrl,
                Version = options.Version,
                Currency = options.Currency,
                MerchantId = options.MerchantId
            };
            return Task.FromResult(merchantInfo);
        }

        public async Task<RetrieveOrderResponse> RetrieveOrder(RetrieveOrderRequest request)
        {
            var httpclient = CreateHttpClient();
            var responseMessage = await httpclient.GetAsync($"api/rest/version/{options.Version}/merchant/{options.MerchantId}/order/{request.OrderId}");
            var stream = await responseMessage.Content.ReadAsStreamAsync();
            return await JsonSerializer.DeserializeAsync<RetrieveOrderResponse>(stream);
        }

        public async Task<RefundOrderResponse> RefundeOrder(RefundOrderRequest request)
        {
            var httpclient = CreateHttpClient();
            var content = new StringContent(JsonSerializer.Serialize(request.Refundobject));
            var responseMessage = await httpclient.PutAsync($"api/rest/version/{options.Version}/merchant/{options.MerchantId}/order/{request.OrderId}/transaction/{request.TransactionId}", content);
            var stream = await responseMessage.Content.ReadAsStreamAsync();
            return await JsonSerializer.DeserializeAsync<RefundOrderResponse>(stream);
        }

        public async Task<PayResponse> Pay(PayRequest request)
        {
            var httpclient = CreateHttpClient();
            var jsonSetting = new JsonSerializerOptions { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull };
            var content = new StringContent(JsonSerializer.Serialize(request.Payobject, jsonSetting));
            var responseMessage = await httpclient.PutAsync($"api/rest/version/{options.Version}/merchant/{options.MerchantId}/order/{request.OrderId}/transaction/{request.TransactionId}", content);
            var stream = await responseMessage.Content.ReadAsStreamAsync();
            return await JsonSerializer.DeserializeAsync<PayResponse>(stream);
        }
    }
}
