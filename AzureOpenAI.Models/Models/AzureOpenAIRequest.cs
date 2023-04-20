namespace AzureOpenAI.Helpers.Models
{
    public class AzureOpenAIRequest
    {
        public List<Message> messages { get; set; } = new List<Message>();
    }

    public class Message
    {
        public string role { get; set; }
        public string content { get; set; }
    }
}
