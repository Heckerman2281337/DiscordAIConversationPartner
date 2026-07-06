

namespace DiscordVoiceBotMark.src.Pipeline
{
    internal sealed class SpeechToTextService : ISpeechToTextService
    {
        public SpeechToTextService(HttpClient client) 
        {
            _httpClient = client;
        }

        private readonly HttpClient _httpClient;
        private readonly string? ApiKey = Environment.GetEnvironmentVariable("STT_API_KEY");

        public Task<string?> ExecuteSpeechToTextAsync(byte[] data)
        {
            throw new NotImplementedException();
        }
    }
}
