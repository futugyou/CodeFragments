using AspnetcoreEx.Resources;
using AspnetcoreEx.SemanticKernel;
using Microsoft.SemanticKernel.AI.ChatCompletion;
using Microsoft.SemanticKernel.AI.ImageGeneration;
using Microsoft.SemanticKernel.AI.TextCompletion;
using Microsoft.SemanticKernel.Connectors.AI.OpenAI.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.AI.OpenAI.Tokenizers;
using Microsoft.SemanticKernel.CoreSkills;
using Microsoft.SemanticKernel.Orchestration;
using Microsoft.SemanticKernel.Planning;
using Microsoft.SemanticKernel.SemanticFunctions;
using Microsoft.SemanticKernel.SkillDefinition;
using Microsoft.SemanticKernel.Skills.OpenAPI.Authentication;
using Microsoft.SemanticKernel.TemplateEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Globalization;

namespace AspnetcoreEx.Controllers;

[Route("api/sk")]
[ApiController]
public class SemanticKernelController : ControllerBase
{
    private readonly IKernel kernel;
    private readonly SemanticKernelOptions options;

    public SemanticKernelController(IKernel kernel, IOptionsMonitor<SemanticKernelOptions> optionsMonitor)
    {
        this.kernel = kernel;
        this.options = optionsMonitor.CurrentValue;
    }

    [Route("start")]
    [HttpPost]
    public async Task<string[]> StartDemo()
    {
        var prompt = @"{{$input}}

One line TLDR with the fewest words.";

        var summarize = kernel.CreateSemanticFunction(prompt);

        string text1 = @"
1st Law of Thermodynamics - Energy cannot be created or destroyed.
2nd Law of Thermodynamics - For a spontaneous process, the entropy of the universe increases.
3rd Law of Thermodynamics - A perfect crystal at zero Kelvin has zero entropy.";

        string text2 = @"
1. An object at rest remains at rest, and an object in motion remains in motion at constant speed and in a straight line unless acted on by an unbalanced force.
2. The acceleration of an object depends on the mass of the object and the amount of force applied.
3. Whenever one object exerts a force on another object, the second object exerts an equal and opposite on the first.";

        var text1result = await summarize.InvokeAsync(text1);

        var text2result = await summarize.InvokeAsync(text2);

        string translationPrompt = @"{{$input}}

Translate the text to math.";

        string summarizePrompt = @"{{$input}}

Give me a TLDR with the fewest words.";

        var translator = kernel.CreateSemanticFunction(translationPrompt);
        summarize = kernel.CreateSemanticFunction(summarizePrompt);

        string inputText = @"
1st Law of Thermodynamics - Energy cannot be created or destroyed.
2nd Law of Thermodynamics - For a spontaneous process, the entropy of the universe increases.
3rd Law of Thermodynamics - A perfect crystal at zero Kelvin has zero entropy.";

        // Run two prompts in sequence (prompt chaining)
        var text3result = await kernel.RunAsync(inputText, translator, summarize);

        return new string[] { text1result.Result, text2result.Result, text3result.Result };
    }


    [Route("init-function")]
    [HttpPost]
    public async Task<string> InlineFunction()
    {
        string skPrompt = """
{{$input}}

Summarize the content above.
""";
        var promptConfig = new PromptTemplateConfig
        {
            Completion = {
                MaxTokens = 2000,
                Temperature = 0.2,
                TopP = 0.5,
            }
        };

        var promptTemplate = new PromptTemplate(
            skPrompt,                        // Prompt template defined in natural language
            promptConfig,                    // Prompt configuration
            kernel                           // SK instance
        );

        var functionConfig = new SemanticFunctionConfig(promptConfig, promptTemplate);

        var summaryFunction = kernel.RegisterSemanticFunction("MySkill", "Summary", functionConfig);

        var input = """
Demo (ancient Greek poet)
From Wikipedia, the free encyclopedia
Demo or Damo (Greek: Δεμώ, Δαμώ; fl. c. AD 200) was a Greek woman of the Roman period, known for a single epigram, engraved upon the Colossus of Memnon, which bears her name. She speaks of herself therein as a lyric poetess dedicated to the Muses, but nothing is known of her life.[1]
Identity
Demo was evidently Greek, as her name, a traditional epithet of Demeter, signifies. The name was relatively common in the Hellenistic world, in Egypt and elsewhere, and she cannot be further identified. The date of her visit to the Colossus of Memnon cannot be established with certainty, but internal evidence on the left leg suggests her poem was inscribed there at some point in or after AD 196.[2]
Epigram
There are a number of graffiti inscriptions on the Colossus of Memnon. Following three epigrams by Julia Balbilla, a fourth epigram, in elegiac couplets, entitled and presumably authored by "Demo" or "Damo" (the Greek inscription is difficult to read), is a dedication to the Muses.[2] The poem is traditionally published with the works of Balbilla, though the internal evidence suggests a different author.[1]
In the poem, Demo explains that Memnon has shown her special respect. In return, Demo offers the gift for poetry, as a gift to the hero. At the end of this epigram, she addresses Memnon, highlighting his divine status by recalling his strength and holiness.[2]
Demo, like Julia Balbilla, writes in the artificial and poetic Aeolic dialect. The language indicates she was knowledgeable in Homeric poetry—'bearing a pleasant gift', for example, alludes to the use of that phrase throughout the Iliad and Odyssey.[a][2] 
""";

        var summary = await kernel.RunAsync(input, summaryFunction);

        Console.WriteLine(summary);

        return summary.Result;
    }

    [Route("contextVar")]
    [HttpPost]
    public async Task<string> ContextVar()
    {
        const string skPrompt = @"
ChatBot can have a conversation with you about any topic.
It can give explicit instructions or say 'I don't know' if it does not have an answer.

{{$history}}
Human: {{$human_input}}
ChatBot:";

        var promptConfig = new PromptTemplateConfig
        {
            Completion =
            {
                MaxTokens = 2000,
                Temperature = 0.7,
                TopP = 0.5,
            }
        };

        var promptTemplate = new PromptTemplate(skPrompt, promptConfig, kernel);
        var functionConfig = new SemanticFunctionConfig(promptConfig, promptTemplate);
        var chatFunction = kernel.RegisterSemanticFunction("ChatBot", "Chat", functionConfig);

        var context = new ContextVariables();

        var history = "";
        context.Set("history", history);

        var human_input = "Hi, I'm looking for book suggestions";
        context.Set("human_input", human_input);

        var bot_answer = await kernel.RunAsync(context, chatFunction);
        history += $"\nHuman: {human_input}\nMelody: {bot_answer}\n";
        context.Update(history);

        Console.WriteLine(context);

        Func<string, Task> Chat = async (string input) =>
        {
            // Save new message in the context variables
            context.Set("human_input", input);

            // Process the user message and get an answer
            var answer = await kernel.RunAsync(context, chatFunction);

            // Append the new interaction to the chat history
            history += $"\nHuman: {input}\nMelody: {answer}\n";
            context.Set("history", history);
            //context.Update(history);
            // Show the response
            Console.WriteLine(context);
        };

        await Chat("I would like a non-fiction book suggestion about Greece history. Please only list one book.");
        await Chat("that sounds interesting, what are some of the topics I will learn about?");
        await Chat("Which topic from the ones you listed do you think most people find interesting?");
        await Chat("could you list some more books I could read about the topic(s) you mentioned?");

        return context.Input;
    }

    [Route("skill")]
    [HttpPost]
    public async Task<string> Skill()
    {
        var mySkill = kernel.Skills;
        var input = """
Console.WriteLine("OK");
""";
        var summary = await kernel.RunAsync(input, mySkill.GetFunction("Skills", "ToGolang"));

        Console.WriteLine(summary);

        return summary.Result;
    }

    [Route("native")]
    [HttpPost]
    public async IAsyncEnumerable<string> Native()
    {
        var mySkill = kernel.Skills;

        var myContext = new ContextVariables();
        myContext.Set("INPUT", "This is input.");

        var myOutput = await kernel.RunAsync(myContext, mySkill.GetFunction("MyCSharpSkill", "DupDup"));
        yield return myOutput.Result;

        myContext = new ContextVariables();
        myContext.Set("firstname", "Sam");
        myContext.Set("lastname", "Appdev");
        myOutput = await kernel.RunAsync(myContext, mySkill.GetFunction("MyCSharpSkill", "FullNamer"));

        yield return myOutput.Result;
    }

    [Route("call-native")]
    [HttpPost]
    public async IAsyncEnumerable<string> CallNative()
    {
        var myContext = new ContextVariables("*Twinnify");
        var mySemSkill = kernel.Skills;
        var myOutput = await kernel.RunAsync(myContext, mySemSkill.GetFunction("Skills", "CallNative"));

        yield return myOutput.Result;
    }

    [Route("call-semantic")]
    [HttpPost]
    public async IAsyncEnumerable<string> CallSemantic()
    {
        var mySkill = kernel.Skills;
        var myContext = new ContextVariables();
        myContext.Set("input", """
Console.WriteLine("OK");
""");

        var myOutput = await kernel.RunAsync(myContext, mySkill.GetFunction("MyCSharpSkill", "togolang"));

        yield return myOutput.Result;
    }

    [Route("core-time")]
    [HttpPost]
    public async IAsyncEnumerable<string> CoreTime()
    {
        const string ThePromptTemplate = @"
Today is: {{time.Date}}
Current time is: {{time.Time}}

Answer to the following questions using JSON syntax, including the data used.
Is it morning, afternoon, evening, or night (morning/afternoon/evening/night)?
Is it weekend time (weekend/not weekend)?";

        var myKindOfDay = kernel.CreateSemanticFunction(ThePromptTemplate, maxTokens: 150);

        var myOutput = await myKindOfDay.InvokeAsync();

        yield return myOutput.Result;
    }

    [Route("core-text")]
    [HttpPost]
    public async IAsyncEnumerable<string> CoreText()
    {
        var myText = kernel.Skills;

        SKContext myOutput = await kernel.RunAsync(
    "    i n f i n i t e     s p a c e     ",
    myText.GetFunction("TrimStart"),
    myText.GetFunction("TrimEnd"),
    myText.GetFunction("Uppercase"));
        Console.WriteLine(myOutput);

        yield return myOutput.Result;
    }

    [Route("core-summary")]
    [HttpPost]
    public async IAsyncEnumerable<string> CoreSummary()
    {
        var myText = kernel.Skills;
        const string ThePromptTemplate = @"There are lots of different ways to say this, but fundamentally, the models are stronger when they are being asked to reason about meaning and goals, and weaker when they are being asked to perform specific calculations and processes. For example, it's easy for advanced models to write code to solve a sudoku generally, but hard for them to solve a sudoku themselves. Each kind of code has different strengths and it's important to use the right kind of code for the right kind of problem. The boundaries between syntax and semantics are the hard parts of these programs.";

        SKContext myOutput = await kernel.RunAsync(ThePromptTemplate, myText.GetFunction("SummarizeConversation"));
        yield return myOutput.Result;

        SKContext myOutput1 = await kernel.RunAsync(ThePromptTemplate, myText.GetFunction("GetConversationActionItems"));
        yield return myOutput1.Result;

        SKContext myOutput2 = await kernel.RunAsync(ThePromptTemplate, myText.GetFunction("GetConversationTopics"));
        yield return myOutput2.Result;
    }

    [Route("core-file")]
    [HttpPost]
    public async IAsyncEnumerable<string> CoreFile()
    {
        var myText = kernel.Skills;
        const string ThePromptTemplate = @"There are lots of different ways to say this, but fundamentally, the models are stronger when they are being asked to reason about meaning and goals, and weaker when they are being asked to perform specific calculations and processes. For example, it's easy for advanced models to write code to solve a sudoku generally, but hard for them to solve a sudoku themselves. Each kind of code has different strengths and it's important to use the right kind of code for the right kind of problem. The boundaries between syntax and semantics are the hard parts of these programs.";

        var myContext = new ContextVariables();
        myContext["path"] = "./fileskilldemo.txt";
        myContext["content"] = ThePromptTemplate;
        SKContext myOutput = await kernel.RunAsync(myContext, myText.GetFunction("file", "WriteAsync"));
        yield return myOutput.Result;

        SKContext myOutput1 = await kernel.RunAsync("./fileskilldemo.txt", myText.GetFunction("file", "ReadAsync"));
        yield return myOutput1.Result;
    }

    [Route("core-http")]
    [HttpPost]
    public async IAsyncEnumerable<string> CoreHttp()
    {
        var myText = kernel.Skills;
        var input = "https://store.steampowered.com/search/suggest?term=tales&f=games&cc=JP&use_store_query=1&use_search_spellcheck=1&search_creators_and_tags=1";
        SKContext myOutput = await kernel.RunAsync(input, default(CancellationToken), myText.GetFunction("http", "GetAsync"));
        yield return myOutput.Result;
    }

    [Route("core-math")]
    [HttpPost]
    public async IAsyncEnumerable<string> CoreMath()
    {
        var myContext = new ContextVariables("90");

        myContext["Amount"] = "10";
        SKContext myOutput = await kernel.RunAsync(myContext, kernel.Func("math", "Add"));
        yield return myOutput.Result; // 100

        myContext["Amount"] = "20";
        SKContext myOutput1 = await kernel.RunAsync(myContext, kernel.Func("math", "Subtract"));
        yield return myOutput.Result; // 80
    }

    [Route("google")]
    [HttpPost]
    public async IAsyncEnumerable<string> Google()
    {
        var question = "What's the largest building in the world?";
        var googleResult = await kernel.Func("google", "search").InvokeAsync(question);
        yield return googleResult.Result;
    }

    [Route("google2")]
    [HttpPost]
    public async IAsyncEnumerable<string> Google2()
    {
        yield return ("======== Use Search Skill to answer user questions ========");

        const string SemanticFunction = @"Answer questions only when you know the facts or the information is provided.
When you don't have sufficient information you reply with a list of commands to find the information needed.
When answering multiple questions, use a bullet point list.
Note: make sure single and double quotes are escaped using a backslash char.

[COMMANDS AVAILABLE]
- google.search

[INFORMATION PROVIDED]
{{ $externalInformation }}

[EXAMPLE 1]
Question: what's the biggest lake in Italy?
Answer: Lake Garda, also known as Lago di Garda.

[EXAMPLE 2]
Question: what's the biggest lake in Italy? What's the smallest positive number?
Answer:
* Lake Garda, also known as Lago di Garda.
* The smallest positive number is 1.

[EXAMPLE 3]
Question: what's Ferrari stock price ? Who is the current number one female tennis player in the world?
Answer:
{{ '{{' }} google.search ""what\\'s Ferrari stock price?"" {{ '}}' }}.
{{ '{{' }} google.search ""Who is the current number one female tennis player in the world?"" {{ '}}' }}.

[END OF EXAMPLES]

[TASK]
Question: {{ $input }}.
Answer: ";

        var questions = "Who is the most followed person on TikTok right now? What's the exchange rate EUR:USD?";
        yield return (questions);

        var oracle = kernel.CreateSemanticFunction(SemanticFunction, maxTokens: 200, temperature: 0, topP: 1);

        var context = kernel.CreateNewContext();
        context["externalInformation"] = "";
        context.Variables.Update(questions);
        var answer = await oracle.InvokeAsync(context);
        yield return (answer.Result);
        // If the answer contains commands, execute them using the prompt renderer.
        if (answer.Result.Contains("google.search", StringComparison.OrdinalIgnoreCase))
        {
            var promptRenderer = new PromptTemplateEngine();

            yield return ("---- Fetching information from google...");
            var information = await promptRenderer.RenderAsync(answer.Result, context);

            yield return ("Information found:");
            yield return (information);

            // The rendered prompt contains the information retrieved from search engines
            context["externalInformation"] = information;

            // Run the semantic function again, now including information from google
            context.Variables.Update(questions);
            answer = await oracle.InvokeAsync(context);
        }
        else
        {
            yield return ("AI had all the information, no need to query google.");
        }

        yield return ("---- ANSWER:");
        yield return (answer.Result);
    }

    [Route("search-url")]
    [HttpPost]
    public async IAsyncEnumerable<string> SearchUrl()
    {
        var ask = "What's the tallest building in Europe?";
        yield return ask;

        var result = await kernel.RunAsync(
            ask,
            kernel.Func("search", "BingSearchUrl")
        );

        yield return (result.Result);
    }

    [Route("planner")]
    [HttpPost]
    public async IAsyncEnumerable<string> Planner()
    {
        var planner = new SequentialPlanner(kernel);
        var goal = "Write a slogan about Coca Cola, then write a golang code about who to send http request.";
        yield return goal;

        var plan = await planner.CreatePlanAsync(goal);
        var result = await kernel.RunAsync(plan);
        yield return result.Result;
    }

    [Route("planner2")]
    [HttpPost]
    public async IAsyncEnumerable<string> Planner2()
    {
        var context = kernel.CreateNewContext();
        context.Variables.Set("firstname", "Jamal");
        context.Variables.Set("lastname", "Williams");
        context.Variables.Set("city", "Tacoma");
        context.Variables.Set("state", "WA");
        context.Variables.Set("country", "USA");

        List<string> memoriesToSave = new()
        {
            "I like pizza and chicken wings.",
            "I ate pizza 10 times this month.",
            "I ate chicken wings 3 time this month.",
            "I ate sushi 1 time this month.",
            "My partner likes sushi and chicken wings.",
            "I like to eat dinner with my partner.",
            "I am a software engineer.",
            "I live in Tacoma, WA.",
            "I have a dog named Tully.",
            "I have a cat named Butters.",
        };

        foreach (var memoryToSave in memoriesToSave)
        {
            await kernel.Memory.SaveInformationAsync("contextQueryMemories", memoryToSave, Guid.NewGuid().ToString());
        }

        var plan = new Plan("Execute ContextQuery and then RunMarkup");
        plan.AddSteps(kernel.Func("Skills", "ContextQuery"), kernel.Func("markup", "RunMarkup"));

        // Execute plan
        context.Variables.Update("Who is my president? Who was president 3 years ago? What should I eat for dinner");
        var result = await plan.InvokeAsync(context);

        yield return result.Result;
    }

    [Route("chat")]
    [HttpPost]
    public async IAsyncEnumerable<string> Chat()
    {
        IChatCompletion chatGPT = kernel.GetService<IChatCompletion>();

        var chatHistory = (OpenAIChatHistory)chatGPT.CreateNewChat("You are a librarian, expert about books");

        // First user message
        chatHistory.AddUserMessage("Hi, I'm looking for book suggestions");
        var message1 = chatHistory.Messages.Last();
        yield return message1.Content;

        // First bot assistant message
        string reply = await chatGPT.GenerateMessageAsync(chatHistory);
        chatHistory.AddAssistantMessage(reply);
        var message2 = chatHistory.Messages.Last();
        yield return message2.Content;
    }

    [Route("chat2")]
    [HttpPost]
    public async IAsyncEnumerable<string> Chat2()
    {
        var systemPromptTemplate = EmbeddedResource.Read("30-system-prompt.txt");
        var selectedText = EmbeddedResource.Read("30-user-context.txt");
        var userPromptTemplate = EmbeddedResource.Read("30-user-prompt.txt");

        var context = kernel.CreateNewContext();
        context["selectedText"] = selectedText;
        context["startTime"] = DateTimeOffset.Now.ToString("hh:mm:ss tt zz", CultureInfo.CurrentCulture);
        context["userMessage"] = "extract locations as a bullet point list";

        var promptRenderer = new PromptTemplateEngine();
        string systemMessage = await promptRenderer.RenderAsync(systemPromptTemplate, context);
        yield return systemMessage;

        string userMessage = await promptRenderer.RenderAsync(userPromptTemplate, context);
        yield return userMessage;

        IChatCompletion chatGPT = kernel.GetService<IChatCompletion>();
        var chatHistory = chatGPT.CreateNewChat(systemMessage);
        chatHistory.AddMessage(ChatHistory.AuthorRoles.User, userMessage);
        string answer = await chatGPT.GenerateMessageAsync(chatHistory);
        yield return answer;

    }

    [Route("dallE")]
    [HttpPost]
    public async IAsyncEnumerable<string> DallE()
    {
        IImageGeneration dallE = kernel.GetService<IImageGeneration>();

        var imageDescription = "A cute baby sea otter";
        var image = await dallE.GenerateImageAsync(imageDescription, 256, 256);

        yield return image;
    }


    [Route("github")]
    [HttpPost]
    public async IAsyncEnumerable<string> Github()
    {
        var authenticationProvider = new BearerAuthenticationProvider(() => { return Task.FromResult(options.GithubToken); });

        var skill = await kernel.ImportOpenApiSkillFromFileAsync(
            "GitHubSkill",
            "./SemanticKernel/openapi.json",
            authenticationProvider.AuthenticateRequestAsync);

        // Add arguments for required parameters, arguments for optional ones can be skipped.
        var contextVariables = new ContextVariables();
        contextVariables.Set("owner", "futugyou");
        contextVariables.Set("repo", "goproject");

        // Run
        var result = await kernel.RunAsync(contextVariables, skill["PullsList"]);

        Console.WriteLine("Successful GitHub List Pull Requests skill response.");
        var resultJson = JsonConvert.DeserializeObject<Dictionary<string, object>>(result.Result);
        var pullRequests = JArray.Parse((string)resultJson!["content"]);

        if (pullRequests != null && pullRequests.First != null)
        {
            var number = pullRequests.First["number"];
            yield return number?.ToString() ?? string.Empty;
        }
        else
        {
            yield return "No pull requests found.";
        }
    }

    [Route("tokenizer")]
    [HttpPost]
    public Task<Dictionary<string, int>> Tokenizer()
    {
        var result = new Dictionary<string, int> {
            { "Some text on one line",GPT3Tokenizer.Encode( "Some text on one line").Count },
            { "⭐⭐",GPT3Tokenizer.Encode( "⭐⭐").Count },
            { "Some text on\ntwo lines",GPT3Tokenizer.Encode( "Some text on\ntwo lines").Count },
            { "Some text on\r\ntwo lines",GPT3Tokenizer.Encode( "Some text on\r\ntwo lines").Count },
        };

        return Task.FromResult(result);
    }

    [Route("jira")]
    [HttpPost]
    public async IAsyncEnumerable<string> Jira()
    {
        var contextVariables = new ContextVariables();

        // Change <your-domain> to a jira instance you have access to with your authentication credentials
        string serverUrl = options.JiraAddress + "rest/api/latest/";
        contextVariables.Set("server-url", serverUrl);

        var tokenProvider = new BasicAuthenticationProvider(() =>
        {
            string s = options.JiraEmailAddress + ":" + options.JiraApiKey;
            return Task.FromResult(s);
        });

        using HttpClient httpClient = new HttpClient();

        var apiSkillRawFileURL = new Uri("https://raw.githubusercontent.com/microsoft/PowerPlatformConnectors/dev/certified-connectors/JIRA/apiDefinition.swagger.json");
        IDictionary<string, ISKFunction> jiraSkills = await kernel.ImportOpenApiSkillFromUrlAsync("jiraSkills", apiSkillRawFileURL, httpClient, tokenProvider.AuthenticateRequestAsync);


        // GetIssue Skill
        {
            // Set Properties for the Get Issue operation in the openAPI.swagger.json
            contextVariables.Set("issueKey", "TEC-42");

            // Run operation via the semantic kernel
            var result = await kernel.RunAsync(contextVariables, jiraSkills["GetIssue"]);

            yield return "\n\n\n";
            var formattedContent = JsonConvert.SerializeObject(JsonConvert.DeserializeObject(result.Result), Formatting.Indented);
            Console.WriteLine(formattedContent);
            yield return "GetIssue jiraSkills response: \n " + formattedContent;
        }

        // AddComment Skill
        {
            // Set Properties for the AddComment operation in the openAPI.swagger.json
            contextVariables.Set("issueKey", "TEC-42");
            contextVariables.Set("body", "Here is a rad comment");
            contextVariables.Set("payload", "Here is a rad comment");

            // Run operation via the semantic kernel
            var result = await kernel.RunAsync(contextVariables, jiraSkills["AddComment"]);

            yield return "\n\n\n";
            var formattedContent = JsonConvert.SerializeObject(JsonConvert.DeserializeObject(result.Result), Formatting.Indented);
            yield return "AddComment jiraSkills response: \n " + formattedContent;
        }
    }


    [Route("streamchat")]
    [HttpPost]
    public async IAsyncEnumerable<string> StreamChat()
    {
        IChatCompletion chatGPT = kernel.GetService<IChatCompletion>();

        var chatHistory = (OpenAIChatHistory)chatGPT.CreateNewChat("You are a librarian, expert about books");

        var message = chatHistory.Messages.Last();
        yield return $"{message.AuthorRole}: {message.Content}";

        // First user message
        chatHistory.AddUserMessage("Hi, I'm looking for book suggestions");
        message = chatHistory.Messages.Last();
        yield return $"{message.AuthorRole}: {message.Content}";

        // First bot assistant message
        string fullMessage = string.Empty;
        await foreach (string assistantMessage in chatGPT.GenerateMessageStreamAsync(chatHistory))
        {
            fullMessage += assistantMessage;
            yield return assistantMessage;
        }
        chatHistory.AddMessage(ChatHistory.AuthorRoles.Assistant, fullMessage);

        // Second user message
        chatHistory.AddUserMessage("I love history and philosophy, I'd like to learn something new about Greece, any suggestion?");
        message = chatHistory.Messages.Last();
        yield return $"{message.AuthorRole}: {message.Content}";

        // Second bot assistant message
        fullMessage = string.Empty;
        await foreach (string assistantMessage in chatGPT.GenerateMessageStreamAsync(chatHistory))
        {
            fullMessage += assistantMessage;
            yield return assistantMessage;
        }
        chatHistory.AddMessage(ChatHistory.AuthorRoles.Assistant, fullMessage);
    }

    [Route("streamcompletion")]
    [HttpPost]
    public async IAsyncEnumerable<string> StreamCompletion()
    {
        ITextCompletion textCompletion = kernel.GetService<ITextCompletion>();
        var requestSettings = new CompleteRequestSettings()
        {
            MaxTokens = 100,
            FrequencyPenalty = 0,
            PresencePenalty = 0,
            Temperature = 1,
            TopP = 0.5
        };

        var prompt = "Write one paragraph why AI is awesome";

        yield return "Prompt: " + prompt;
        await foreach (string message in textCompletion.CompleteStreamAsync(prompt, requestSettings))
        {
            yield return message;
        }
    }

    //[Route("multicompletion")]
    //[HttpPost]
    //public async IAsyncEnumerable<string> MultiCompletion()
    //{
    //    ITextCompletion textCompletion = kernel.GetService<ITextCompletion>();
    //    var requestSettings = new CompleteRequestSettings()
    //    {
    //        MaxTokens = 200,
    //        FrequencyPenalty = 0,
    //        PresencePenalty = 0,
    //        Temperature = 1,
    //        TopP = 0.5,
    //        ResultsPerPrompt = 2,
    //    };

    //    var prompt = "Write one paragraph why AI is awesome";

    //    foreach (ITextCompletionResult completionResult in await textCompletion.GetCompletionsAsync(prompt, requestSettings))
    //    {
    //        yield return await completionResult.GetCompletionAsync();
    //        yield return "-------------";
    //    }
    //}

    [Route("memory")]
    [HttpPost]
    public async IAsyncEnumerable<string> Memory()
    {
        string MemoryCollectionName = "aboutMe";
        // ========= Store memories using the kernel =========

        await kernel.Memory.SaveInformationAsync(MemoryCollectionName, id: "info1", text: "My name is Andrea");
        await kernel.Memory.SaveInformationAsync(MemoryCollectionName, id: "info2", text: "I work as a tourist operator");
        await kernel.Memory.SaveInformationAsync(MemoryCollectionName, id: "info3", text: "I've been living in Seattle since 2005");
        await kernel.Memory.SaveInformationAsync(MemoryCollectionName, id: "info4", text: "I visited France and Italy five times since 2015");

        // ========= Store memories using semantic function =========

        // Build a semantic function that saves info to memory
        const string SaveFunctionDefinition = @"{{save $info}}";
        var memorySaver = kernel.CreateSemanticFunction(SaveFunctionDefinition);

        var context = kernel.CreateNewContext();
        context[TextMemorySkill.CollectionParam] = MemoryCollectionName;
        context[TextMemorySkill.KeyParam] = "info5";
        context["info"] = "My family is from New York";
        await memorySaver.InvokeAsync(context);

        // 1
        context[TextMemorySkill.KeyParam] = "info1";
        var answer = await kernel.Func("baseMemory", "Retrieve").InvokeAsync(context);
        yield return "Memory associated with 'info1': " + answer;
        yield return "";

        // 2
        context.Variables.Update("where did I grow up?");
        context[TextMemorySkill.LimitParam] = "2";
        answer = await kernel.Func("baseMemory", "Recall").InvokeAsync(context);
        yield return ("Ask: where did I grow up?");
        yield return (answer.Result);
        yield return "";

        context.Variables.Update("where do I live?");
        answer = await kernel.Func("baseMemory", "Recall").InvokeAsync(context);
        yield return ("Ask: where do I live?");
        yield return answer.Result;

        // 3

    }
}