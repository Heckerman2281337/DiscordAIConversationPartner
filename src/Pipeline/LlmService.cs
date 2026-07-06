using DotNetEnv;
using Microsoft.Extensions.Logging;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Net.Http.Json;
using System.Text.Json;

namespace DiscordVoiceBotMark.src.Pipeline
{
    internal sealed class LlmService : ILlmService
    {
        public LlmService(HttpClient client, ILogger<LlmService> logger)
        {
            _httpClient = client;
            _logger = logger;

            _apiKey = Environment.GetEnvironmentVariable("OPENROUTER_API_KEY");
            if (_apiKey == null) _logger.Log(LogLevel.Error, $"[LlmService] Нет api для openrouter");

            _endpoint = Environment.GetEnvironmentVariable("LLM_API_ENDPOINT");
            if (_endpoint == null) _logger.Log(LogLevel.Error, $"[LlmService] Нет endpoint для openrouter");

            _systemPromt = Environment.GetEnvironmentVariable("BOT_SYSTEM_PROMPT");
            if (_systemPromt == null)
            {
                _systemPromt = "Ты полезный ИИ-ассистент.";
                _logger.Log(LogLevel.Error, $"[LlmService] Нет BOT_SYSTEM_PROMPT, установлен дефолтный");
            }
        }

        private readonly HttpClient _httpClient;
        private readonly ILogger<LlmService> _logger;
        private readonly string? _apiKey;
        private readonly string? _endpoint;
        private readonly string? _systemPromt; 
        public async Task<string?> ExecuteLlmAsync(List<object> history)
        {
            if(history == null || history.Count == 0) return null;

            //data for API
            var messagesPayload = new List<object>
            {
                new { role = "system", content = _systemPromt! }
            };

            messagesPayload.AddRange(history);

            var payload = new
            {
                model = "meta-llama/llama-3-8b-instruct:free",
                messages = messagesPayload
            };

            //HTTP requests
            var request = new HttpRequestMessage(HttpMethod.Post, _endpoint);
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _apiKey);
            request.Content = JsonContent.Create(payload);

            HttpResponseMessage respones = await _httpClient.SendAsync(request);

            if (!respones.IsSuccessStatusCode)
            {
                string errorLog = await respones.Content.ReadAsStringAsync();
                _logger.Log(LogLevel.Error, $"[LlmService] {errorLog}");
                return null;
            }

            using JsonDocument document = await JsonDocument.ParseAsync(await respones.Content.ReadAsStreamAsync());

            if (document.RootElement.TryGetProperty("choices", out JsonElement choicesElement) &&
                choicesElement.GetArrayLength() > 0)
            {
                JsonElement firstChoice = choicesElement[0];

                if (firstChoice.TryGetProperty("message", out JsonElement messageElement))
                {
                    if(messageElement.TryGetProperty("content", out JsonElement contentElement))
                    {
                        return contentElement.GetString();
                    }
                }
            }

            return null;
        }
    }
}
