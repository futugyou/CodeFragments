using Contracts;
using MassTransit;
using MassTransit.Mediator;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sender.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValueController : ControllerBase
    {
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly IMediator _mediator;

        public ValueController(IPublishEndpoint publishEndpoint, IMediator mediator)
        {
            _publishEndpoint = publishEndpoint;
            _mediator = mediator;
        }

        [HttpPost]
        public async Task<ActionResult> Post(string value)
        {
            await _publishEndpoint.Publish<ValueEntered>(new
            {
                Value = value
            });
            return Ok();
        }

        [HttpPut]
        public async Task<ActionResult> Put(string value)
        {
            Guid orderId = NewId.NextGuid();
            await _mediator.Send<SubmitOrder>(new { OrderId = orderId });
            return Ok();
        }
    }
}
