

using AgentStack.Agents;
using AgentStack.Tools;

[JsonSerializable(typeof(Recipe))]
[JsonSerializable(typeof(Ingredient))]
[JsonSerializable(typeof(RecipeResponse))]
[JsonSerializable(typeof(Plan))]
[JsonSerializable(typeof(Step))]
[JsonSerializable(typeof(StepStatus))]
[JsonSerializable(typeof(StepStatus?))] 
[JsonSerializable(typeof(List<string>))]
[JsonSerializable(typeof(System.Int32))]
[JsonSerializable(typeof(JsonPatchOperation))]
[JsonSerializable(typeof(List<JsonPatchOperation>))]
public sealed partial class AguiJsonSerializerContext : JsonSerializerContext;