

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
public sealed partial class AguiJsonSerializerContext : JsonSerializerContext;