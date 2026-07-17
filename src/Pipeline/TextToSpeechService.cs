
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;

namespace DiscordVoiceBotMark.src.Pipeline
{
    internal sealed class XttsService : ITextToSpeechService
    {
        public XttsService(HttpClient client, ILogger<XttsService> logger) 
        { 
            _httpClient = client;
            _logger = logger;

            _endpoint = Environment.GetEnvironmentVariable("XTTS_API_ENDPOINT");
            if (_endpoint == null)
            {
                _logger.Log(LogLevel.Error, $"[XttsService] Нет XTTS_API_ENDPOINT");
                return;
            }

            _sampleFilePath = Environment.GetEnvironmentVariable("XTTS_SPEAKER_WAV");
            if (_sampleFilePath == null)
            {
                _logger.Log(LogLevel.Warning, $"[XttsService] Нет _sampleFilePath");
                return;
            }
        }

        private readonly HttpClient _httpClient;
        private readonly ILogger<XttsService> _logger;
        private readonly string? _endpoint;
        private readonly string? _sampleFilePath;

        public async Task<byte[]?> ExecuteTextToSpeechAsync(string aiAnswer)
        {
            if (string.IsNullOrWhiteSpace(aiAnswer)) return null;

            var payload = new
            {
                text = aiAnswer,
                speaker_wav = _sampleFilePath,
                language = "ru"
            };

            var request = new HttpRequestMessage(HttpMethod.Post, _endpoint);
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
