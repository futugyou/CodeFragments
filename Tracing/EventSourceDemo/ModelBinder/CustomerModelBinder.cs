using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EventSourceDemo.ModelBinder
{
    public class CustomerModelBinder : IModelBinder
    {
        private readonly BodyModelBinder bodyModelBinder;

        public CustomerModelBinder(FormatterCollection<IInputFormatter> inputFormatters, IHttpRequestStreamReaderFactory httpRequestStreamReaderFactory)
        {
            bodyModelBinder = new BodyModelBinder(inputFormatters, httpRequestStreamReaderFactory);
        }

        /// <summary>
        /// this is demo
        /// </summary>
        /// <param name="bindingContext"></param>
        /// <returns></returns>
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            bodyModelBinder.BindModelAsync(bindingContext);
            var request = bindingContext.ActionContext.HttpContext.Request;
            if (request.HttpContext.Items["somecode"] != null)
            {
                //TODO: do something
            }
            {
                if (!bindingContext.Result.IsModelSet)
                {
                    return Task.CompletedTask;
                }
                var modelName = bindingContext.ModelName;

                var model = bindingContext.Result.Model;
                {
                    //TODO: do something
                }
            }
            return Task.CompletedTask;
        }
    }

    public class CustomModelBinderProvider : IModelBinderProvider
    {
        private MvcOptions options;

        public CustomModelBinderProvider(MvcOptions options)
        {
            this.options = options;
        }

        public IModelBinder GetBinder(ModelBinderProviderContext context)
        {
            if (context.BindingInfo.BindingSource != null &&
                   context.BindingInfo.BindingSource.CanAcceptDataFrom(BindingSource.Body))
            {
                //通过[FromBody]绑定的
                return new CustomerModelBinder(options.InputFormatters, context.Services.GetRequiredService<IHttpRequestStreamReaderFactory>());
            }
            return null;
        }
    }
}
