
namespace AgentStack.Tools;

public static class AgenticPlanningTools
{
    [Description("Create a plan with multiple steps.")]
    public static Plan CreatePlan([Description("List of step descriptions to create the plan.")] List<string> steps)
    {
        return new Plan
        {
            Steps = [.. steps.Select(s => new Step { Description = s, Status = StepStatus.Pending })]
        };
    }

    [Description("Update a step in the plan with new description or status.")]
    public static async Task<List<JsonPatchOperation>> UpdatePlanStepAsync(
        [Description("The index of the step to update.")] int index,
        [Description("The new description for the step (optional).")] string? description = null,
        [Description("The new status for the step (optional).")] StepStatus? status = null)
    {
        var changes = new List<JsonPatchOperation>();

        if (description is not null)
        {
            changes.Add(new JsonPatchOperation
            {
                Op = "replace",
                Path = $"/steps/{index}/description",
                Value = description
            });
        }

        if (status.HasValue)
        {
            // Status must be lowercase to match AG-UI frontend expectations: "pending" or "completed"
            string statusValue = status.Value == StepStatus.Pending ? "pending" : "completed";
            changes.Add(new JsonPatchOperation
            {
                Op = "replace",
                Path = $"/steps/{index}/status",
                Value = statusValue
            });
        }

        await Task.Delay(1000);

        return changes;
    }
}

public sealed class JsonPatchOperation
{
    [JsonPropertyName("op")]
    public required string Op { get; set; }

    [JsonPropertyName("path")]
    public required string Path { get; set; }

    [JsonPropertyName("value")]
    public object? Value { get; set; }

    [JsonPropertyName("from")]
    public string? From { get; set; }
}

public sealed class Plan
{
    [JsonPropertyName("steps")]
    public List<Step> Steps { get; set; } = [];
}

public sealed class Step
{
    [JsonPropertyName("description")]
    public required string Description { get; set; }

    [JsonPropertyName("status")]
    public StepStatus Status { get; set; } = StepStatus.Pending;
}


[JsonConverter(typeof(JsonStringEnumConverter<StepStatus>))]
public enum StepStatus
{
    Pending,
    Completed
}