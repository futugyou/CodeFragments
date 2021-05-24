using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace EventSourceDemo
{
    public class ApiRequestTimeFilterAttribute : ActionFilterAttribute
    {
        readonly Stopwatch _stopwatch = new Stopwatch();
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            base.OnActionExecuting(context);
            //开启耗时记录
            _stopwatch.Start();
        }
        public override void OnResultExecuted(ResultExecutedContext context)
        {
            //关闭耗时记录
            _stopwatch.Stop();
            //调用方法记录耗时 　　　　
            ApiEventCounterSource.Log.Request(context.HttpContext.Request.GetDisplayUrl(), _stopwatch.ElapsedMilliseconds);
        }
    }
}
