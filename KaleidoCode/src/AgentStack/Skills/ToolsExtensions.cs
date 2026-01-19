
namespace AgentStack.Skills;

using System.ComponentModel;
using System.Text.Json.Serialization;

public class ToolsExtensions
{

    public static AITool[] GetAIToolsFromType<T>() where T : class, new()
    {
        var obj = new T();
        return Creator(obj);
    }

    public static AITool[] GetAIToolsFromType<T>(IServiceProvider serviceProvider) where T : class
    {
        T obj = serviceProvider.GetRequiredService<T>();
        return Creator(obj);
    }

    private static AITool[] Creator<T>(T obj) where T : class
    {
        return [
            .. typeof(T)
                .GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.DeclaredOnly)
                .Select((m) => AIFunctionFactory.Create(m, target: null)), // Get from type static methods
            .. obj.GetType()
                .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
                .Select((m) => AIFunctionFactory.Create(m, target: obj)) // Get from instance methods
                ];
    }
}