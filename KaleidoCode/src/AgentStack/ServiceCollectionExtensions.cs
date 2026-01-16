
using AgentStack.Services;
using AgentStack.Executor;
using AgentStack.ContextProvider;
using AgentStack.Skills;
using AgentStack.ThreadStore;
using AgentStack.Middleware;
using AgentStack.Model;
using Microsoft.SemanticKernel.Connectors.PgVector;
using Microsoft.Agents.AI.Hosting;
using AgentStack.MessageStore;
using Microsoft.Extensions.VectorData;
using Npgsql;

namespace AgentStack;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAgentServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<AgentOptions>(configuration.GetSection("Agent"));
        var connectionString = configuration.GetConnectionString("Postgres")!;

        services.AddKeyedSingleton<OpenAIClient>("AgentOpenAIClient", (sp, _) =>
        {
            var _options = sp.GetRequiredService<IOptionsMonitor<AgentOptions>>().CurrentValue;
            var credential = new ApiKeyCredential(_options.TextCompletion.ApiKey);
            OpenAIClientOptions openAIOptions = new();
            if (!string.IsNullOrEmpty(_options.TextCompletion.Endpoint))
            {
                openAIOptions.Endpoint = new Uri(_options.TextCompletion.Endpoint);
            }

            return new OpenAIClient(credential, openAIOptions);
        });

        services.AddKeyedChatClient("AgentChatClient", (sp) =>
        {
            var client = sp.GetRequiredKeyedService<OpenAIClient>("AgentOpenAIClient");
            var _options = sp.GetRequiredService<IOptionsMonitor<AgentOptions>>().CurrentValue;
            return client.GetChatClient(_options.TextCompletion.ModelId).AsIChatClient();
        });

        services.AddSingleton<IEmbeddingGenerator<string, Embedding<float>>>(sp =>
        {
            var loggerFactory = sp.GetService<ILoggerFactory>();
            var _options = sp.GetRequiredService<IOptionsMonitor<AgentOptions>>().CurrentValue;
            OpenAIClientOptions clientOption = new() { Endpoint = new Uri(_options.Embedding.Endpoint) };

            var builder = new OpenAIClient(
                   credential: new ApiKeyCredential(_options.Embedding.ApiKey),
                   options: clientOption)
               .GetEmbeddingClient(_options.Embedding.ModelId)
               .AsIEmbeddingGenerator(_options.Embedding.Dimensions)
               .AsBuilder();

            if (loggerFactory is not null)
            {
                builder.UseLogging(loggerFactory);
            }

            return builder.Build();
        });

        services.AddSingleton(sp =>
        {
            var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
            dataSourceBuilder.UseVector();
            return dataSourceBuilder.Build();
        });

        services.AddSingleton<PostgresAgentThreadStore>();
        services.AddSingleton<AgentThreadStore>(sp => sp.GetRequiredService<PostgresAgentThreadStore>());

        services.AddKeyedPostgresVectorStore("AgentVectorStore",
            connectionStringProvider: _ => connectionString,
            optionsProvider: sp =>
            {
                var embeddingGenerator = sp.GetRequiredService<IEmbeddingGenerator<string, Embedding<float>>>();
                return new PostgresVectorStoreOptions()
                {
                    EmbeddingGenerator = embeddingGenerator,
                };
            });

        services.AddSingleton<AIContextProviderFactory>();

        // it is not ok, beacuse it is Singleton, but can not change to scope, AddAIAgent is Singleton
        // services.AddKeyedSingleton<AIContextProvider>("AgentContextProvider", (sp, _) =>
        // {
        //     var store = sp.GetRequiredKeyedService<VectorStore>("AgentVectorStore");
        //     var httpContext = sp.GetRequiredService<IHttpContextAccessor>().HttpContext;
        //     var request = httpContext?.Request;
        //     foreach (var item in request?.Headers)
        //     {
        //         Console.WriteLine($"{item.Key}: {item.Value}");
        //     }
        //     var threadId = request?.Headers != null ? request?.Headers["ThreadId"].FirstOrDefault() : "";
        //     var userId = request?.Headers != null ? request?.Headers["UserId"].FirstOrDefault() : "";

        //     Console.WriteLine($"!!!!{threadId}: {userId}");
        //     return new ChatHistoryMemoryProvider(
        //         store,
        //         collectionName: "chathistory_memory",
        //         vectorDimensions: 1536,
        //         storageScope: new() { UserId = userId, ThreadId = threadId },
        //         searchScope: new() { UserId = userId });
        // });

        services.AddScoped<AgentService>();
        services.AddScoped<WorkflowService>();

        // AddAIAgent will use AddSingleton
        services.AddAIAgent("joker", (sp, name) =>
        {
            var chatClient = sp.GetRequiredKeyedService<IChatClient>("AgentChatClient");
            chatClient = chatClient
                .AsBuilder()
                .Use(getResponseFunc: AgentMiddleware.ChatClientMiddleware, getStreamingResponseFunc: AgentMiddleware.ChatClientStreamMiddleware)
                .Build();

            var providerFactory = sp.GetRequiredService<AIContextProviderFactory>();
            var vectorStore = sp.GetRequiredKeyedService<VectorStore>("AgentVectorStore");
            return chatClient.CreateAIAgent(new ChatClientAgentOptions
            {
                Name = "joker",
                ChatOptions = new() { Instructions = "You are good at telling jokes." },
                ChatMessageStoreFactory = ctx =>
                {
                    return new VectorChatMessageStore(
                       vectorStore,
                       ctx.SerializedState,
                       ctx.JsonSerializerOptions);
                },
                AIContextProviderFactory = providerFactory.Create
            });
        });

        AITool[] lightTools = ToolsExtensions.GetAIToolsFromType<LightPlugin>();

        services.AddAIAgent("light", (sp, name) =>
        {
            var chatClient = sp.GetRequiredKeyedService<IChatClient>("AgentChatClient");

            return chatClient.CreateAIAgent(
                new ChatClientAgentOptions
                {
                    Name = "light",
                    ChatOptions = new()
                    {
                        Instructions = "You are a useful light assistant. can tell user the status of the lights and can help user control the lights on and off .",
                        Tools = lightTools,
                    }
                }
            );
        });

        // The overload of AddAIAgent that includes `sp` is ineffective with WithAITools because the `GetRegisteredToolsForAgent(sp, name)` method is not called. 
        // .WithAITools(tools)

        // `WithThreadStore` doesn't work either. 
        // The current source code only uses `AgentThreadStore` indirectly within Hosting.A2A, and it's not yet implemented in DevUI/Hosting.OpenAI. 
        // .WithThreadStore((sp, name) =>
        // {
        //     var store = sp.GetRequiredService<AgentThreadStore>();
        //     return store;
        // });

        var lightPlugin = new LightPlugin();
        AITool[] lightToolsWithApproval = [
            AIFunctionFactory.Create(lightPlugin.GetLightsAsync),
            // Upon testing, devui failed to recognize ApprovalRequiredAIFunction and executed the command directly.
            new ApprovalRequiredAIFunction(AIFunctionFactory.Create(lightPlugin.ChangeStateAsync))
        ];
        services.AddAIAgent("light-with-approval", (sp, name) =>
        {
            var chatClient = sp.GetRequiredKeyedService<IChatClient>("AgentChatClient");

            return chatClient.CreateAIAgent(
                new ChatClientAgentOptions
                {
                    Name = "light-with-approval",
                    ChatOptions = new()
                    {
                        Instructions = "You are a useful light assistant. can tell user the status of the lights and can help user control the lights on and off .",
                        Tools = lightTools,
                    }
                }
            );
        });

        services.AddAIAgent("rag", (sp, name) =>
        {
            var vectorStore = sp.GetRequiredKeyedService<VectorStore>("AgentVectorStore");
            var vectorCollection = vectorStore.GetCollection<string, SemanticSearchRecord>(SemanticSearchRecord.GetCollectionName());
            async Task<IEnumerable<TextSearchProvider.TextSearchResult>> SearchAdapter(string text, CancellationToken ct)
            {
                List<TextSearchProvider.TextSearchResult> results = [];
                await foreach (var result in vectorCollection.SearchAsync(text, 5, cancellationToken: ct))
                {
                    results.Add(new TextSearchProvider.TextSearchResult
                    {
                        SourceName = result.Record.FileName,
                        SourceLink = "",
                        Text = result.Record.Text ?? string.Empty,
                        RawRepresentation = result
                    });
                }
                return results;
            }

            var chatClient = sp.GetRequiredKeyedService<IChatClient>("AgentChatClient");
            TextSearchProviderOptions textSearchOptions = new()
            {
                SearchTime = TextSearchProviderOptions.TextSearchBehavior.BeforeAIInvoke,
                RecentMessageMemoryLimit = 5
            };

            return chatClient.CreateAIAgent(
                new ChatClientAgentOptions
                {
                    Name = "rag",
                    ChatOptions = new()
                    {
                        Instructions = "You are a helpful support specialist for the Microsoft Agent Framework. Answer questions using the provided context and cite the source document when available. Keep responses brief.",
                    },
                    AIContextProviderFactory = ctx => new TextSearchProvider(SearchAdapter, ctx.SerializedState, ctx.JsonSerializerOptions, textSearchOptions)
                }
            );
        });

        // One errors:
        // 1. v1/conversations?entity_id=sequential&type=workflow_session will get `agent_id query parameter is required.`
        // The Executor used in devui must implement `ChatProtocolExecutor` or accept a `List<ChatMessage>`.
        services.AddAIWorkflow("sequential", (sp, key) =>
        {
            if (key is not string keyString)
            {
                throw new InvalidOperationException("The expected name is null.");
            }

            UppercaseChatProtocolExecutor uppercase = new();
            ReverseChatProtocolExecutor reverse = new();
            AppendSuffixChatProtocolExecutor append = new(" [PROCESSED]");
            WorkflowBuilder builder = new(uppercase);
            builder.WithName(keyString);
            builder.AddEdge(uppercase, reverse);
            builder.AddEdge(reverse, append);
            builder.WithOutputFrom(append);
            return builder.Build();
        });

        // One errors:
        // 1. v1/conversations?entity_id=sequential&type=workflow_session will get `agent_id query parameter is required.` 
        services.AddAIWorkflow("concurrent", (sp, key) =>
        {
            if (key is not string keyString)
            {
                throw new InvalidOperationException("The expected name is null.");
            }

            var chatClient = sp.GetRequiredKeyedService<IChatClient>("AgentChatClient");
            AIAgent physicist = chatClient.CreateAIAgent(
                name: "Physicist",
                instructions: "You are an expert in physics. You answer questions from a physics perspective."
            );

            AIAgent chemist = chatClient.CreateAIAgent(
                name: "Chemist",
                instructions: "You are an expert in chemistry. You answer questions from a chemistry perspective."
            );
            return AgentWorkflowBuilder.BuildConcurrent(workflowName: keyString, agents: [physicist, chemist]);
        });

        services.AddAIWorkflow("handoff", (sp, key) =>
        {
            if (key is not string keyString)
            {
                throw new InvalidOperationException("The expected name is null.");
            }

            var chatClient = sp.GetRequiredKeyedService<IChatClient>("AgentChatClient");
            ChatClientAgent historyTutor = new(chatClient,
                "You provide assistance with historical queries. Explain important events and context clearly. Only respond about history.",
                "history_tutor",
                "Specialist agent for historical questions");
            ChatClientAgent mathTutor = new(chatClient,
                "You provide help with math problems. Explain your reasoning at each step and include examples. Only respond about math.",
                "math_tutor",
                "Specialist agent for math questions");
            ChatClientAgent triageAgent = new(chatClient,
                "You determine which agent to use based on the user's homework question. ALWAYS handoff to another agent.",
                "triage_agent",
                "Routes messages to the appropriate specialist agent");
            var workflow = AgentWorkflowBuilder.CreateHandoffBuilderWith(triageAgent)
            .WithHandoffs(triageAgent, [mathTutor, historyTutor])
            .WithHandoffs([mathTutor, historyTutor], triageAgent)
            .Build();
            // The workflow class does not have a `setName` method, so reflection is used. 
            workflow.WithName(keyString);
            return workflow;
        });

        services.AddAIWorkflow("groupChat", (sp, key) =>
        {
            if (key is not string keyString)
            {
                throw new InvalidOperationException("The expected name is null.");
            }

            var chatClient = sp.GetRequiredKeyedService<IChatClient>("AgentChatClient");
            static ChatClientAgent GetTranslationAgent(string targetLanguage, IChatClient chatClient) =>
            new(chatClient, $"You are a translation assistant who only responds in {targetLanguage}. Respond to any " + $"input by outputting the name of the input language and then translating the input to {targetLanguage}.");

            var workflow = AgentWorkflowBuilder.CreateGroupChatBuilderWith(agents => new RoundRobinGroupChatManager(agents) { MaximumIterationCount = 3 })
            .AddParticipants(from lang in (string[])["French", "Spanish", "English"] select GetTranslationAgent(lang, chatClient))
            .Build();

            workflow.WithName(keyString);
            return workflow;
        });

        services.AddAIWorkflow("sub-workflow", (sp, key) =>
        {
            if (key is not string keyString)
            {
                throw new InvalidOperationException("The expected name is null.");
            }

            // Using dependency injection (DI) can lead to ownership issues.
            // var subWorkflow = sp.GetRequiredKeyedService<Workflow>("sequential");

            UppercaseChatProtocolExecutor uppercase = new();
            ReverseChatProtocolExecutor reverse = new();
            AppendSuffixChatProtocolExecutor append = new(" [PROCESSED]");
            WorkflowBuilder builder = new(uppercase);
            builder.WithName(keyString);
            builder.AddEdge(uppercase, reverse);
            builder.AddEdge(reverse, append);
            builder.WithOutputFrom(append);
            var subWorkflow = builder.Build();

            // Testing revealed that the subworkflow does not complete successfully; it gets stuck somewhere in the middle.
            // BindAsExecutor is highly suspected to be the cause.
            // TODO: https://github.com/microsoft/agent-framework/issues/2911
            // There are also many other issues related to subworkflow.
            ExecutorBinding subWorkflowExecutor = subWorkflow.BindAsExecutor("TextProcessingSubWorkflow");

            PrefixChatProtocolExecutor prefix = new("INPUT: ");
            PostProcessProtocolExecutor postProcess = new();

            var workflow = new WorkflowBuilder(prefix)
                .WithName(keyString)
                .AddEdge(prefix, subWorkflowExecutor)
                .AddEdge(subWorkflowExecutor, postProcess)
                .WithOutputFrom(postProcess)
                .Build();

            return workflow;
        });

        services.AddAIWorkflow("route-switch", (sp, key) =>
        {
            if (key is not string keyString)
            {
                throw new InvalidOperationException("The expected name is null.");
            }

            PrefixChatProtocolExecutor prefix = new("INPUT: ");
            UppercaseChatProtocolExecutor uppercase = new();
            ReverseChatProtocolExecutor reverse = new();
            PostProcessProtocolExecutor postProcess = new();

            WorkflowBuilder builder = new(prefix);
            builder.WithName(keyString);
            builder.AddSwitch(prefix, sw => sw
            // Do a simple test of the `Switch/Case` in DevUI.
                 .AddCase<CriticDecision>(cd => false, uppercase)
                 .AddCase<CriticDecision>(cd => true, reverse))
             .AddEdge(uppercase, postProcess)
             .AddEdge(reverse, postProcess)
             .WithOutputFrom(postProcess);

            return builder.Build();
        });

        services.AddOpenAIResponses();
        services.AddOpenAIConversations();


        return services;
    }

    public static IServiceCollection AddAIWorkflow(this IServiceCollection services, string name, Func<IServiceProvider, string, Workflow> createWorkflowDelegate)
    {
        services.AddKeyedSingleton(name, (sp, key) =>
        {
            if (key is not string keyString)
            {
                throw new InvalidOperationException("The expected name is null.");
            }
            return createWorkflowDelegate(sp, keyString);
        });
        services.AddAIAgent(name, (sp, name) =>
        {
            var workflow = sp.GetRequiredKeyedService<Workflow>(name);
            return workflow.AsAgent(id: name, name: name);
        });

        return services;
    }
}
