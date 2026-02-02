namespace AgentStack.ContextProvider;

// Python has ready-made solutions, but in .NET have to find out how to use multiple AIContextProviders from the official examples.
// https://github.com/microsoft/agent-framework/blob/98cd72839e4057d661a58092a3b013993264d834/dotnet/samples/GettingStarted/Agents/Agent_Step20_AdditionalAIContext/Program.cs#L166
public sealed class AggregatingAIContextProvider : AIContextProvider
{
    private readonly List<AIContextProvider> _providers = [];

    public AggregatingAIContextProvider(ProviderFactory[] providerFactories, JsonElement jsonElement, JsonSerializerOptions? jsonSerializerOptions)
    {
        // We received a json object, so let's check if it has some previously serialized state that we can use.
        if (jsonElement.ValueKind == JsonValueKind.Object)
        {
            _providers = [.. providerFactories.Select(factory => factory.FactoryMethod(jsonElement.TryGetProperty(factory.ProviderType.Name, out var prop) ? prop : default, jsonSerializerOptions))];
            return;
        }

        // We didn't receive any valid json, so we can just construct fresh providers.
        _providers = [.. providerFactories.Select(factory => factory.FactoryMethod(default, jsonSerializerOptions))];
    }

    public override async ValueTask<AIContext> InvokingAsync(InvokingContext context, CancellationToken cancellationToken = default)
    {
        // Invoke all the sub providers.
        var tasks = _providers.Select(provider => provider.InvokingAsync(context, cancellationToken).AsTask());
        var results = await Task.WhenAll(tasks);

        // Combine the results from each sub provider.
        return new AIContext
        {
            Tools = [.. results.SelectMany(r => r.Tools ?? [])],
            Messages = [.. results.SelectMany(r => r.Messages ?? [])],
            Instructions = string.Join("\n", results.Select(r => r.Instructions).Where(s => !string.IsNullOrEmpty(s)))
        };
    }

    public override JsonElement Serialize(JsonSerializerOptions? jsonSerializerOptions = null)
    {
        Dictionary<string, JsonElement> elements = [];
        foreach (var provider in _providers)
        {
            JsonElement element = provider.Serialize(jsonSerializerOptions);

            // Don't try to store state for any providers that aren't producing any.
            if (element.ValueKind != JsonValueKind.Undefined && element.ValueKind != JsonValueKind.Null)
            {
                elements[provider.GetType().Name] = element;
            }
        }

        return JsonSerializer.SerializeToElement(elements, jsonSerializerOptions);
    }

    public static ProviderFactory CreateFactory<TProviderType>(Func<JsonElement, JsonSerializerOptions?, TProviderType> factoryMethod)
        where TProviderType : AIContextProvider => new()
        {
            FactoryMethod = (jsonElement, jsonSerializerOptions) => factoryMethod(jsonElement, jsonSerializerOptions),
            ProviderType = typeof(TProviderType)
        };

    public readonly struct ProviderFactory
    {
        public Func<JsonElement, JsonSerializerOptions?, AIContextProvider> FactoryMethod { get; init; }

        public Type ProviderType { get; init; }
    }
}