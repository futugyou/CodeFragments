using GreenPipes;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using System;

namespace Sender
{
    public class HttpContextScopeFilter : IFilter<PublishContext>, IFilter<SendContext>, IFilter<ConsumeContext>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public HttpContextScopeFilter(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        private void AddPayload(PipeContext context)
        {
            if (_httpContextAccessor.HttpContext == null)
                return;

            var serviceProvider = _httpContextAccessor.HttpContext.RequestServices;
            context.GetOrAddPayload(() => serviceProvider);
            context.GetOrAddPayload<IServiceScope>(() => new NoopScope(serviceProvider));
        }

        public Task Send(PublishContext context, IPipe<PublishContext> next)
        {
            AddPayload(context);
            return next.Send(context);
        }

        public Task Send(SendContext context, IPipe<SendContext> next)
        {
            AddPayload(context);
            return next.Send(context);
        }

        public Task Send(ConsumeContext context, IPipe<ConsumeContext> next)
        {
            AddPayload(context);
            return next.Send(context);
        }

        public void Probe(ProbeContext context)
        {
        }

        private class NoopScope :
            IServiceScope
        {
            public NoopScope(IServiceProvider serviceProvider)
            {
                ServiceProvider = serviceProvider;
            }

            public void Dispose()
            {
            }

            public IServiceProvider ServiceProvider { get; }
        }
    }

}