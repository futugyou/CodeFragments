
using AgentStack.Skills;
using AgentStack.Services;
using Microsoft.AspNetCore.Mvc;

namespace AgentStack.Api;

public static class WorkflowEndpoints
{
    public static void UseWorkflowEndpoints(this IEndpointRouteBuilder app)
    {
        var agentGroup = app.MapGroup("/api/workflow")
                .WithName("maf workflow");

        agentGroup.MapPost("/sequential", Sequential).WithName("sequential");
        agentGroup.MapPost("/concurrent", Concurrent).WithName("concurrent");
        agentGroup.MapPost("/handoffs", Handoffs).WithName("handoffs");
        agentGroup.MapPost("/groupchat", Groupchat).WithName("groupchat");
        agentGroup.MapPost("/sub-workflow", SubWorkflow).WithName("sub-workflow");
        agentGroup.MapPost("/switch-routes", SwitchRoutes).WithName("switch-routes");
        agentGroup.MapPost("/checkpoint", Checkpoint).WithName("checkpoint");
    }

    static IAsyncEnumerable<string> Sequential([FromServices] WorkflowService workflowService, bool streaming = false)
    {
        return workflowService.Sequential(streaming);
    }

    static IAsyncEnumerable<string> Concurrent([FromServices] WorkflowService workflowService)
    {
        return workflowService.Concurrent();
    }

    static IAsyncEnumerable<string> Handoffs([FromServices] WorkflowService workflowService, string question = "What is the square root of 2?")
    {
        return workflowService.Handoffs(question);
    }

    static IAsyncEnumerable<string> Groupchat([FromServices] WorkflowService workflowService, string input = "Excellent! This slogan is clear, impactful, and directly communicates the key benefits")
    {
        return workflowService.Groupchat(input);
    }

    static IAsyncEnumerable<string> SubWorkflow([FromServices] WorkflowService workflowService, string input = "directly communicates the key benefits")
    {
        return workflowService.SubWorkflow(input);
    }

    static IAsyncEnumerable<string> SwitchRoutes([FromServices] WorkflowService workflowService, string input = "Write a 200-word blog post about AI ethics. Make it thoughtful and engaging.")
    {
        return workflowService.SwitchRoutes(input, 3);
    }

    static IAsyncEnumerable<string> Checkpoint([FromServices] WorkflowService workflowService)
    {
        return workflowService.Checkpoint();
    }

}