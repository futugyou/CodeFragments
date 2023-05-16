using AspnetcoreEx.SemanticKernel;
using AspnetcoreEx.SemanticKernel.Skills;
using Microsoft.SemanticKernel.CoreSkills;
using Microsoft.SemanticKernel.Orchestration;
using Microsoft.SemanticKernel.SemanticFunctions;
using Microsoft.SemanticKernel.Skills.Web;
using Microsoft.SemanticKernel.Skills.Web.Google;
using Microsoft.SemanticKernel.TemplateEngine;

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
        kernel.Config.AddOpenAITextCompletionService(
            "text-davinci-003",
            options.Key
        );

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
        kernel.Config.AddOpenAITextCompletionService(
            "text-davinci-003",
            options.Key
        );

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
        kernel.Config.AddOpenAITextCompletionService(
            "text-davinci-003",
            options.Key
        );

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
        kernel.Config.AddOpenAITextCompletionService(
            "text-davinci-003",
            options.Key
        );

        var mySkill = kernel.ImportSemanticSkillFromDirectory("SemanticKernel", "Skills");
        var input = """
Console.WriteLine("OK");
""";
        var summary = await kernel.RunAsync(input, mySkill["ToGolang"]);

        Console.WriteLine(summary);

        return summary.Result;
    }

    [Route("native")]
    [HttpPost]
    public async Task<string> Native()
    {
        kernel.Config.AddOpenAITextCompletionService(
            "text-davinci-003",
            options.Key
        );

        var mySkill = kernel.ImportSkill(new SteamSkill(), "MyCSharpSkill");

        var myContext = new ContextVariables();
        myContext.Set("INPUT", "This is input.");

        var myOutput = await kernel.RunAsync(myContext, mySkill["DupDup"]);
        Console.WriteLine(myOutput);

        myContext = new ContextVariables();
        myContext.Set("firstname", "Sam");
        myContext.Set("lastname", "Appdev");
        myOutput = await kernel.RunAsync(myContext, mySkill["FullNamer"]);

        Console.WriteLine(myOutput);

        return myOutput.Result;
    }

    [Route("call-native")]
    [HttpPost]
    public async Task<string> CallNative()
    {
        kernel.Config.AddOpenAITextCompletionService(
            "text-davinci-003",
            options.Key
        );

        var myContext = new ContextVariables("*Twinnify");
        kernel.ImportSkill(new SteamSkill(), "MyCSharpSkill");
        var mySemSkill = kernel.ImportSemanticSkillFromDirectory("SemanticKernel", "Skills");
        var myOutput = await kernel.RunAsync(myContext, mySemSkill["CallNative"]);
        Console.WriteLine(myOutput);

        return myOutput.Result;
    }

    [Route("call-semantic")]
    [HttpPost]
    public async Task<string> CallSemantic()
    {
        kernel.Config.AddOpenAITextCompletionService(
            "text-davinci-003",
            options.Key
        );

        var mySkill = kernel.ImportSkill(new SteamSkill(), "MyCSharpSkill");
        kernel.ImportSemanticSkillFromDirectory("SemanticKernel", "Skills");
        var myContext = new ContextVariables();
        myContext.Set("input", """
Console.WriteLine("OK");
""");

        var myOutput = await kernel.RunAsync(myContext, mySkill["togolang"]);
        Console.WriteLine(myOutput);

        return myOutput.Result;
    }

    [Route("core-time")]
    [HttpPost]
    public async Task<string> CoreTime()
    {
        kernel.Config.AddOpenAITextCompletionService(
            "text-davinci-003",
            options.Key
        );

        var mySkill = kernel.ImportSkill(new TimeSkill(), "time");

        const string ThePromptTemplate = @"
Today is: {{time.Date}}
Current time is: {{time.Time}}

Answer to the following questions using JSON syntax, including the data used.
Is it morning, afternoon, evening, or night (morning/afternoon/evening/night)?
Is it weekend time (weekend/not weekend)?";

        var myKindOfDay = kernel.CreateSemanticFunction(ThePromptTemplate, maxTokens: 150);

        var myOutput = await myKindOfDay.InvokeAsync();
        Console.WriteLine(myOutput);

        return myOutput.Result;
    }

    [Route("core-text")]
    [HttpPost]
    public async Task<string> CoreText()
    {
        kernel.Config.AddOpenAITextCompletionService(
            "text-davinci-003",
            options.Key
        );

        var myText = kernel.ImportSkill(new TextSkill());

        SKContext myOutput = await kernel.RunAsync(
    "    i n f i n i t e     s p a c e     ",
    myText["TrimStart"],
    myText["TrimEnd"],
    myText["Uppercase"]);
        Console.WriteLine(myOutput);

        return myOutput.Result;
    }

    [Route("core-summary")]
    [HttpPost]
    public async Task<string[]> CoreSummary()
    {
        kernel.Config.AddOpenAITextCompletionService(
            "text-davinci-003",
            options.Key
        );

        var myText = kernel.ImportSkill(new ConversationSummarySkill(kernel));
        const string ThePromptTemplate = @"There are lots of different ways to say this, but fundamentally, the models are stronger when they are being asked to reason about meaning and goals, and weaker when they are being asked to perform specific calculations and processes. For example, it's easy for advanced models to write code to solve a sudoku generally, but hard for them to solve a sudoku themselves. Each kind of code has different strengths and it's important to use the right kind of code for the right kind of problem. The boundaries between syntax and semantics are the hard parts of these programs.";

        SKContext myOutput = await kernel.RunAsync(ThePromptTemplate, myText["SummarizeConversation"]);
        SKContext myOutput1 = await kernel.RunAsync(ThePromptTemplate, myText["GetConversationActionItems"]);
        SKContext myOutput2 = await kernel.RunAsync(ThePromptTemplate, myText["GetConversationTopics"]);

        return new string[] { myOutput.Result, myOutput1.Result, myOutput2.Result };
    }

    [Route("core-file")]
    [HttpPost]
    public async Task<string[]> CoreFile()
    {
        kernel.Config.AddOpenAITextCompletionService(
            "text-davinci-003",
            options.Key
        );

        var myText = kernel.ImportSkill(new FileIOSkill(), "file");
        const string ThePromptTemplate = @"There are lots of different ways to say this, but fundamentally, the models are stronger when they are being asked to reason about meaning and goals, and weaker when they are being asked to perform specific calculations and processes. For example, it's easy for advanced models to write code to solve a sudoku generally, but hard for them to solve a sudoku themselves. Each kind of code has different strengths and it's important to use the right kind of code for the right kind of problem. The boundaries between syntax and semantics are the hard parts of these programs.";

        var myContext = new ContextVariables();
        myContext["path"] = "./fileskilldemo.txt";
        myContext["content"] = ThePromptTemplate;
        SKContext myOutput = await kernel.RunAsync(myContext, myText["WriteAsync"]);
        SKContext myOutput1 = await kernel.RunAsync("./fileskilldemo.txt", myText["ReadAsync"]);

        return new string[] { myOutput.Result, myOutput1.Result };
    }

    [Route("core-http")]
    [HttpPost]
    public async Task<string[]> CoreHttp()
    {
        kernel.Config.AddOpenAITextCompletionService(
            "text-davinci-003",
            options.Key
        );

        var myText = kernel.ImportSkill(new HttpSkill(), "http");

        var input = "https://store.steampowered.com/search/suggest?term=tales&f=games&cc=JP&use_store_query=1&use_search_spellcheck=1&search_creators_and_tags=1";
        SKContext myOutput = await kernel.RunAsync(input, default(CancellationToken), myText["GetAsync"]);

        return new string[] { myOutput.Result };
    }

    [Route("core-math")]
    [HttpPost]
    public async Task<string[]> CoreMath()
    {
        kernel.Config.AddOpenAITextCompletionService(
            "text-davinci-003",
            options.Key
        );

        var myText = kernel.ImportSkill(new MathSkill(), "math");

        var myContext = new ContextVariables("90");

        myContext["Amount"] = "10";
        SKContext myOutput = await kernel.RunAsync(myContext, myText["Add"]);
        Console.WriteLine(myOutput.Result); // 100
        myContext["Amount"] = "20";
        SKContext myOutput1 = await kernel.RunAsync(myContext, myText["Subtract"]);
        Console.WriteLine(myOutput.Result); // 80 

        return new string[] { myOutput.Result, myOutput1.Result };
    }

    [Route("google")]
    [HttpPost]
    public async Task<string[]> Google()
    {
        kernel.Config.AddOpenAITextCompletionService("text-davinci-003", options.Key);

        // Load Google skill
        using var googleConnector = new GoogleConnector(options.GoogleApikey, options.GoogleEngine);
        kernel.ImportSkill(new WebSearchEngineSkill(googleConnector), "google");

        var question = "What's the largest building in the world?";
        var googleResult = await kernel.Func("google", "search").InvokeAsync(question);
        Console.WriteLine(googleResult.Result);

        return new string[] { googleResult.Result };
    }

    [Route("google2")]
    [HttpPost]
    public async Task<string[]> Google2()
    {
        kernel.Config.AddOpenAITextCompletionService("text-davinci-003", options.Key);

        // Load Google skill
        using var googleConnector = new GoogleConnector(options.GoogleApikey, options.GoogleEngine);
        kernel.ImportSkill(new WebSearchEngineSkill(googleConnector), "google");

        Console.WriteLine("======== Use Search Skill to answer user questions ========");

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
        Console.WriteLine(questions);

        var oracle = kernel.CreateSemanticFunction(SemanticFunction, maxTokens: 200, temperature: 0, topP: 1);

        var context = kernel.CreateNewContext();
        context["externalInformation"] = "";
        context.Variables.Update(questions);
        var answer = await oracle.InvokeAsync(context);
        Console.WriteLine(answer);
        // If the answer contains commands, execute them using the prompt renderer.
        if (answer.Result.Contains("google.search", StringComparison.OrdinalIgnoreCase))
        {
            var promptRenderer = new PromptTemplateEngine();

            Console.WriteLine("---- Fetching information from google...");
            var information = await promptRenderer.RenderAsync(answer.Result, context);

            Console.WriteLine("Information found:");
            Console.WriteLine(information);

            // The rendered prompt contains the information retrieved from search engines
            context["externalInformation"] = information;

            // Run the semantic function again, now including information from google
            context.Variables.Update(questions);
            answer = await oracle.InvokeAsync(context);
        }
        else
        {
            Console.WriteLine("AI had all the information, no need to query google.");
        }

        Console.WriteLine("---- ANSWER:");
        Console.WriteLine(answer);

        return new string[] { answer.Result };
    }



    [Route("search-url")]
    [HttpPost]
    public async Task<string> SearchUrl()
    {
        kernel.Config.AddOpenAITextCompletionService("text-davinci-003", options.Key);

        var skill = new SearchUrlSkill();
        var bing = kernel.ImportSkill(skill, "search");

        // Run
        var ask = "What's the tallest building in Europe?";
        var result = await kernel.RunAsync(
            ask,
            bing["BingSearchUrl"]
        );

        Console.WriteLine(ask + "\n");
        Console.WriteLine(result);

        return result.Result;
    }

}