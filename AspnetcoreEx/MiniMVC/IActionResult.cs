using System.Text;
using System.Text.Json;

namespace AspnetcoreEx.MiniMVC;

public interface IActionResult
{
    Task ExecuteResultAsync(ActionContext  actionContext);
}

public class JsonResult(object data) : IActionResult
{
    public Task ExecuteResultAsync(ActionContext actionContext)
    {
        var response = actionContext.HttpContext.Response;
        response.ContentType = "application/json";
        return JsonSerializer.SerializeAsync(response.Body, data);
    }
}