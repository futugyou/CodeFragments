using Elsa.Builders;
using Elsa.Activities.Http;

namespace ElsaCore.Server.Workflows;
public class HelloWorldWorkflow : IWorkflow
{
    public void Build(IWorkflowBuilder builder)
    {
        builder.HttpEndpoint(setup =>
        {
            setup.WithMethod(HttpMethod.Get.Method).WithPath("/code/hello-world");
        })
            .Then<WriteHttpResponse>(setup =>
            {
                setup.WithContentType("text/html")
                .WithContent("<h1>Hello World! </h1><p>这是通过代码实现的流程</p>")
                .WithStatusCode(System.Net.HttpStatusCode.OK);
            });
    }
}
