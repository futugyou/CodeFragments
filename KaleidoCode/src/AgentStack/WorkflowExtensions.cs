
using Microsoft.Agents.AI.Workflows;

namespace AgentStack;

public static class WorkflowExtensions
{
    readonly static FieldInfo? field = typeof(Workflow).GetField("<Name>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic);
    readonly static FieldInfo? privateName = typeof(Workflow).GetField("_name", BindingFlags.Instance | BindingFlags.NonPublic);

    public static Workflow WithName(this Workflow workflow, string name)
    {
        if (field != null)
        {
            field.SetValue(workflow, name);
        }
        else
        {
            privateName?.SetValue(workflow, name);
        }

        Console.WriteLine($"Workflow name: {workflow.Name}");

        return workflow;
    }
}