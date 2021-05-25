using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EventSourceDemo.Controllers
{
    /// <summary>
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class EventsourceController : ControllerBase
    {
        [HttpPost]
        public void SaveCustomer(EventViewModel request)
        {
            var sub = new SubPayload(request.Id, request.Name);
            var payload = new Payload
            {
                Sub = sub,
                KeyValuePairs = new Dictionary<Guid, SubPayload> { { request.Id, sub } },
                SubPayloads = new List<SubPayload> { sub }
            };
            DatabaseEventSource.Instance.PayloadHad(payload);
            DatabaseEventSource.Instance.RegisterComplete();
        }

        [HttpGet]
        public void GetCustomer()
        {
            DatabaseEventSource.Instance.OnCammandExecute(2, "this is a sql");
            DiagnosticObserver.Instance.RegisteDiagnosticObserver();
        }
    }
}
