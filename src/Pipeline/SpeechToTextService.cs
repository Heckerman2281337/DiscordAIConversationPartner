

using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Text.Json;

namespace DiscordVoiceBotMark.src.Pipeline
{
    internal sealed class SpeechToTextService : ISpeechToTextService
    {
        public SpeechToTextService(HttpClient client, ILogger<SpeechToTextService> logger)
        {
            _httpClient = client;
            _logger = logger;

            _apiKey = Environment.GetEnvironmentVariable("OPENROUTER_API_KEY");
            if (_apiKey == null) _logger.Log(LogLevel.Error, $"[SpeechToTextService] Нет api для openrouter");

            _endpoint = Environment.GetEnvironmentVariable("STT_API_ENDPOINT");
            if (_endpoint == null) _logger.Log(LogLevel.Error, $"[SpeechToTextService] Нет endpoint для openrouter");
        }

        private readonly HttpClient _httpClient;
        private readonly ILogger<SpeechToTextService> _logger;
        private readonly string? _apiKey;
        private readonly string? _endpoint;

        public async Task<string?> ExecuteSpeechToTextAsync(byte[] data)
        {
            if (data == null || data.Length == 0) return null;

            var convertedData = Convert.ToBase64String(data);

            //data for API
            var payload = new
            {
                input_audio = new { data = convertedData, format = "mp3" },
                model = "openai/whisper-large-v3",
                language = "ru"
            };
            //HTTP requests
            var request = new HttpRequestMessage(HttpMethod.Post, _endpoint);
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _apiKey);
            request.Content = JsonContent.Create(payload);

            HttpResponseMessage respones = await _httpClient.SendAsync(request);

            if (!respones.IsSuccessStatusCode)
            {
                string errorLog = await respones.Content.ReadAsStringAsync();
                _logger.Log(LogLevel.Error, $"[SpeechToTextService] {errorLog}");
                return null;
            }

            using JsonDocument document = await JsonDocument.ParseAsync(await respones.Content.ReadAsStreamAsync());

            if (document.RootElement.TryGetProperty("text", out JsonElement jsonElement))
            {
                return jsonElement.ToString();
            }

            return null;
        }
    }
}
