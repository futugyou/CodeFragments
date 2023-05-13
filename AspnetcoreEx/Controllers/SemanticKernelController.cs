using AspnetcoreEx.SemanticKernel;

namespace AspnetcoreEx.Controllers
{
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
    }
}
