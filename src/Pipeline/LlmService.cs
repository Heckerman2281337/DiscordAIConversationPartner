using DotNetEnv;

namespace DiscordVoiceBotMark.src.Pipeline
{
    internal sealed class LlmService : ILlmService
    {
        public LlmService(HttpClient client) 
        {
            _httpClient = client;
        }

        private readonly HttpClient _httpClient;
        private readonly string? ApiKey = Environment.GetEnvironmentVariable("LLM_API_KEY");

        public Task<string?> ExecuteLlmAsync(string promt)
        {
            
        }
    }
}
