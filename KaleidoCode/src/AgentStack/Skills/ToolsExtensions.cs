
namespace AgentStack.Skills;

using System.ComponentModel;
using System.Text.Json.Serialization;

public class ToolsExtensions
{

    public static AITool[] GetAIToolsFromType<T>() where T : new()
    {
        var obj = new T();
        return [
            .. typeof(T)
                .GetMethods(BindingFlags.Static | BindingFlags.Public)
                .Select((m) => AIFunctionFactory.Create(m, target: null)), // Get from type static methods
            .. obj.GetType()
                .GetMethods(BindingFlags.Instance | BindingFlags.Public)
                .Select((m) => AIFunctionFactory.Create(m, target: obj)) // Get from instance methods
        ];
    }

    public static AITool[] GetAIToolsFromType<T>(IServiceProvider serviceProvider) where T : notnull
    {
        T obj = serviceProvider.GetRequiredService<T>();
        return [
            .. typeof(T)
                .GetMethods(BindingFlags.Static | BindingFlags.Public)
                .Select((m) => AIFunctionFactory.Create(m, target: null)), // Get from type static methods
            .. obj.GetType()
                .GetMethods(BindingFlags.Instance | BindingFlags.Public)
                .Select((m) => AIFunctionFactory.Create(m, target: obj)) // Get from instance methods
        ];
    }
}