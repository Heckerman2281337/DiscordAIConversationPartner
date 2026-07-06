
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;

namespace DiscordVoiceBotMark.src.Pipeline
{
    internal sealed class TextToSpeechService : ITextToSpeechService
    {
        public TextToSpeechService(HttpClient client, ILogger<TextToSpeechService> logger) 
        { 
            _httpClient = client;
            _logger = logger;

            _apiKey = Environment.GetEnvironmentVariable("FISH_API_KEY");
            if (_apiKey == null) _logger.Log(LogLevel.Error, $"[TextToSpeechService] Нет api для fishaudio");

            _endpoint = Environment.GetEnvironmentVariable("TTS_API_ENDPOINT");
            if (_endpoint == null) _logger.Log(LogLevel.Error, $"[TextToSpeechService] Нет endpoint для fishaudio");

            _modelId = Environment.GetEnvironmentVariable("VOICE_ID");
            if (_modelId == null) _logger.Log(LogLevel.Error, $"[TextToSpeechService] Нет voiceid для fishaudio");
        }

        private readonly HttpClient _httpClient;
        private readonly ILogger<TextToSpeechService> _logger;
        private readonly string? _apiKey;
        private readonly string? _endpoint;
        private readonly string? _modelId;

        public async Task<byte[]?> ExecuteTextToSpeechAsync(string aiAnswer)
        {
            if (string.IsNullOrWhiteSpace(aiAnswer)) return null;

            var payload = new
            {
                text = aiAnswer,
                reference_id = _modelId,
                format = "pcm",
                sample_rate = 48000
            };

            var request = new HttpRequestMessage(HttpMethod.Post, _endpoint);
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _apiKey);
            request.Content = JsonContent.Create(payload);

            HttpResponseMessage respones = await _httpClient.SendAsync(request);

            if (!respones.IsSuccessStatusCode)
            {
                string errorLog = await respones.Content.ReadAsStringAsync();
                _logger.Log(LogLevel.Error, $"[TextToSpeechService] {errorLog}");
                return null;
            }

            return await respones.Content.ReadAsByteArrayAsync();
        }
    }
}
