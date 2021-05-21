using Mastercard.Model.v60;
using Mastercard.Proxy;
using Mastercard.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Mastercard.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private readonly ILogger<WeatherForecastController> _logger;
        private readonly IMastercardProxy _mastercardProxy;

        public WeatherForecastController(ILogger<WeatherForecastController> logger, IMastercardProxy mastercardProxy)
        {
            _logger = logger;
            _mastercardProxy = mastercardProxy;
        }


        [HttpGet]
        public async Task<CreateSessionViewModel> Get()
        {
            var order = new Order
            {
                Id = Guid.NewGuid().ToString().Replace("-", string.Empty),
                Amount = 1M,
                Currency = "THB",
            };
            var model = new CreateSessionViewModel()
            {
                OrderId = order.Id,
                Currency = order.Currency,
            };
            var request = new CreateCheckoutSessionRequest { Order = order };
            var response = await _mastercardProxy.CreateCheckoutSession(request);
            model.Merchant = response.Merchant;
            model.Session = response.Session;
            model.SuccessIndicator = response.SuccessIndicator;
            return model;
        }
    }
}
