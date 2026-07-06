
namespace DiscordVoiceBotMark.src.Pipeline
{
    internal sealed class TextToSpeechService : ITextToSpeechService
    {
        public TextToSpeechService(HttpClient client) 
        { 
            _httpClient = client;
        }

        private readonly HttpClient _httpClient;
        private readonly string? ApiKey = Environment.GetEnvironmentVariable("TTS_API_KEY");

        public Task<byte[]?> ExecuteTextToSpeechAsync(string aiAnswer)
        {
            throw new NotImplementedException();
        }
    }
}
