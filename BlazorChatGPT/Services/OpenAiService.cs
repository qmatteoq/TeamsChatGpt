using BlazorChatGPT.Pages;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.KernelExtensions;
using Microsoft.SemanticKernel.Orchestration;

namespace BlazorChatGPT.Services
{
    public class OpenAiService : IOpenAiService
    {
        private IKernel kernel;
        private readonly IConfiguration _configuration;
        private IDictionary<string, ISKFunction> skill;

        public OpenAiService(IConfiguration configuration)
        {
            _configuration = configuration;

            kernel = Kernel.Builder.Build();
        }

        public void UseOpenAI()
        {
            string openAIKey = _configuration["OpenAI:ApiKey"];
            string model = _configuration["OpenAI:Model"];

            kernel.Config.AddOpenAIChatCompletionService(model, model, openAIKey);
            skill = kernel.ImportSemanticSkillFromDirectory("Skills", "GeneratorSkill");
        }

        public void UseAzureOpenAI()
        {
            string apiKey = _configuration["AzureOpenAI:ApiKey"];
            string deploymentName = _configuration["AzureOpenAI:DeploymentName"];
            string endpoint = _configuration["AzureOpenAI:Endpoint"];

            kernel.Config.AddAzureChatCompletionService("chatgpt-azure", deploymentName, endpoint, apiKey, true);
            skill = kernel.ImportSemanticSkillFromDirectory("Skills", "GeneratorSkill");
        }

        public async Task<string> GenerateSessionAsync(string topic, string tone)
        {
            var context = new ContextVariables();
            context.Set("topic", topic);
            context.Set("tone", tone);

            var result = await kernel.RunAsync(context, skill["SessionGeneratorFunction"]);
            if (!result.ErrorOccurred)
            {
                return result.Result;
            }
            else
            {
                return result.LastErrorDescription;
            }
        }

        public async Task<string> GenerateBusinessReportAsync(string customer, string problem, string solution)
        {
            var context = new ContextVariables();
            context.Set("customer", customer);
            context.Set("problem", problem);
            context.Set("solution", solution);

            var result = await kernel.RunAsync(context, skill["ReportGeneratorFunction"]);

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
            var context = new ContextVariables();
            context.Set("text", text);

            var result = await kernel.RunAsync(context, skill["MailGeneratorFunction"]);

            if (!result.ErrorOccurred)
            {
                return result.Result;
            }
            else
            {
                return result.LastErrorDescription;
            }
        }
    }
}
