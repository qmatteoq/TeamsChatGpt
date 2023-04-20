using AzureOpenAI.Helpers.Models;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Schema;

namespace TeamsChatBot
{
    /// <summary>
    /// An empty bot handler.
    /// You can add your customization code here to extend your bot logic if needed.
    /// </summary>
    public class TeamsBot : TeamsActivityHandler
    {
        private readonly IConfiguration _configuration;

        public TeamsBot(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            string input = turnContext.Activity.Text;

            string uri = _configuration["AzureOpenAIUrl"];
            string apiKey = _configuration["AzureOpenAIKey"];

            AzureOpenAIRequest request = new AzureOpenAIRequest();
            request.messages.Add(new Message { role = "system", content = "You are ChatGPT, a large language model trained by OpenAI. Answer as concisely as possible." });
            request.messages.Add(new Message { role = "user", content = input });

            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("api-key", apiKey);

            var response = await client.PostAsJsonAsync<AzureOpenAIRequest>(uri, request);
            var azureOpenAIResponse = await response.Content.ReadFromJsonAsync<AzureOpenAIResponse>();

            await turnContext.SendActivityAsync(MessageFactory.Text(azureOpenAIResponse.choices.FirstOrDefault().message.content));
        }
    }
}
