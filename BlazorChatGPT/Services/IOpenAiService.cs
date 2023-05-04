namespace BlazorChatGPT.Services
{
    public interface IOpenAiService
    {
        Task<string> GenerateSessionAsync(string topic, string tone);

        Task<string> GenerateBusinessReportAsync(string customer, string problem, string solution);

        Task<string> GenerateBusinessMailAsync(string text);

        void UseAzureOpenAI();
        void UseOpenAI();
    }
}