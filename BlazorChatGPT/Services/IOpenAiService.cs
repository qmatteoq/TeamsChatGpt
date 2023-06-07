namespace BlazorChatGPT.Services
{
    public interface IOpenAiService
    {
        Task<string> GenerateSessionAsync(string topic, string tone);

        Task<string> GenerateSessionInlineAsync(string topic, string tone);

        Task<string> GenerateBusinessReportAsync(string customer, string problem, string solution);

        Task<string> GenerateBusinessMailAsync(string text);

        Task<string> GenerateTranslatedSessionAsync(string topic, string tone);

        Task<string> GenerateStarWarsMail(string character);

        Task<string> GenerateStarWarsDescriptionAsync(string character);

        void UseAzureOpenAI();
        void UseOpenAI();
    }
}