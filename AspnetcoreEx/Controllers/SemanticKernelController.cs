﻿using AspnetcoreEx.SemanticKernel;
using Microsoft.SemanticKernel.Orchestration;
using Microsoft.SemanticKernel.SemanticFunctions;

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

        Func<string, Task> Chat = async (string input) => {
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


}