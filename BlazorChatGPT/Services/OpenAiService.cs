using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Orchestration;
using Microsoft.SemanticKernel.Planning;
using Microsoft.SemanticKernel.SkillDefinition;

namespace BlazorChatGPT.Services
{
    public class OpenAiService : IOpenAiService
    {
        private IKernel kernel;
        private readonly IConfiguration _configuration;
        IDictionary<string, ISKFunction> generatorSkills;
        IDictionary<string, ISKFunction> starWarsSkills;

        public OpenAiService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void UseOpenAI()
        {
            string openAIKey = _configuration["OpenAI:ApiKey"];
            string model = _configuration["OpenAI:Model"];

            var kernelBuilder = new KernelBuilder();
            kernelBuilder.WithOpenAIChatCompletionService(model, openAIKey);

            kernel = kernelBuilder.Build();

            generatorSkills = kernel.ImportSemanticSkillFromDirectory("Skills", "GeneratorSkill");
            starWarsSkills = kernel.ImportSemanticSkillFromDirectory("Skills", "StarWarsSkill");
        }

        public void UseAzureOpenAI()
        {
            string apiKey = _configuration["AzureOpenAI:ApiKey"];
            string deploymentName = _configuration["AzureOpenAI:DeploymentName"];
            string endpoint = _configuration["AzureOpenAI:Endpoint"];

            var kernelBuilder = new KernelBuilder();
            kernelBuilder.WithAzureChatCompletionService(deploymentName, endpoint, apiKey);

            kernel = kernelBuilder.Build();

            generatorSkills = kernel.ImportSemanticSkillFromDirectory("Skills", "GeneratorSkill");
            starWarsSkills = kernel.ImportSemanticSkillFromDirectory("Skills", "StarWarsSkill");
        }

        public async Task<string> GenerateSessionInlineAsync(string topic, string tone)
        {
            var context = new ContextVariables();
            context.Set("topic", topic);
            context.Set("tone", tone);

            string prompt = @"
              Generate a title and an abstract for submitting a presentation for a technical conference. Generate the output as a HTML list, with the following format:

            - Title (make it bold): {title} 
            - Abstract (make it bold): {abstract}

            Use the following tone: {{$tone}}

            The topic of the presentation is defined in the following text enclosed between triple backticks:

            Topic: ```{{$topic}}```  
            ";

            var function = kernel.CreateSemanticFunction(prompt);
            var result = await kernel.RunAsync(context, function);
            return result.Result;
        }

        public async Task<string> GenerateSessionAsync(string topic, string tone)
        {
            var context = new ContextVariables();
            context.Set("topic", topic);
            context.Set("tone", tone);

            var result = await kernel.RunAsync(context, generatorSkills["SessionGeneratorFunction"]);
            if (!result.ErrorOccurred)
            {
                return result.Result;
            }
            else
            {
                return result.LastErrorDescription;
            }
        }

        public async Task<string> GenerateTranslatedSessionAsync(string topic, string tone)
        {
            var context = new ContextVariables();
            context.Set("topic", topic);
            context.Set("tone", tone);

            string firstPrompt = @"
              Generate a title and an abstract for submitting a presentation for a technical conference. Generate the output as a HTML list, with the following format:

            - Title (make it bold): {title} 
            - Abstract (make it bold): {abstract}

            Use the following tone: {{$tone}}

            The topic of the presentation is defined in the following text enclosed between triple backticks:

            Topic: ```{{$topic}}```  
            ";

            string secondPrompt = @"
                Translate the following content in Italian: {{$input}}
            ";

            var myFirstPromptFunction = kernel.CreateSemanticFunction(firstPrompt);
            var mySecondPromptFunction = kernel.CreateSemanticFunction(secondPrompt);

            var output = await kernel.RunAsync(context, myFirstPromptFunction, mySecondPromptFunction);
            return output.Result;
        }

        public async Task<string> GenerateBusinessReportAsync(string customer, string problem, string solution)
        {
            var context = new ContextVariables();
            context.Set("customer", customer);
            context.Set("problem", problem);
            context.Set("solution", solution);

            var result = await kernel.RunAsync(context, generatorSkills["ReportGeneratorFunction"]);

            if (!result.ErrorOccurred)
            {
                return result.Result;
            }
            else
            {
                return result.LastErrorDescription;
            }
        }

        public async Task<string> GenerateBusinessMailAsync(string text)
        {
            var result = await kernel.RunAsync(text, generatorSkills["MailGeneratorFunction"]);

            if (!result.ErrorOccurred)
            {
                return result.Result;
            }
            else
            {
                return result.LastErrorDescription;
            }
        }

        public async Task<string> GenerateStarWarsDescriptionAsync(string character)
        {
            var result = await kernel.RunAsync(character, starWarsSkills["StarWarsFunction"]);

            if (!result.ErrorOccurred)
            {
                return result.Result;
            }
            else
            {
                return result.LastErrorDescription;
            }
        }

        public async Task<string> GenerateStarWarsMail(string character)
        {
            var planner = new SequentialPlanner(kernel);
            var ask = "Write a business mail to describe the following Star Wars character to a customer: {{$input}}";
            var originalPlan = await planner.CreatePlanAsync(ask);

            var originalPlanResult = await kernel.RunAsync(character, originalPlan);

            Console.WriteLine("Original Plan results:\n");
            Console.WriteLine(originalPlanResult.Result);

            return originalPlanResult.Result;
        }
    }
}
