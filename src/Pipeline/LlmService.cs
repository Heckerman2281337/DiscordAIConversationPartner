using DotNetEnv;
using Microsoft.Extensions.Logging;
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


            _endpoint = Environment.GetEnvironmentVariable("LLM_API_ENDPOINT");
            if (_endpoint == null )
            {
                _logger.Log(LogLevel.Error, $"[LlmService] Нет LLM_API_ENDPOINT");
                return;
            }
            
            _modelName = Environment.GetEnvironmentVariable("LLM_MODEL_NAME");
            if ( _modelName == null )
            {
                _modelName = "qwen2:1.5b";
                _logger.Log(LogLevel.Warning, $"[LlmService] Нет LLM_MODEL_NAME, использую дефолтную: {_modelName}");
            }
                
            _systemPromt = Environment.GetEnvironmentVariable("BOT_SYSTEM_PROMPT");
            if (_systemPromt == null)
            {
                _systemPromt = "Ты полезный ИИ-ассистент.";
                _logger.Log(LogLevel.Error, $"[LlmService] Нет BOT_SYSTEM_PROMPT, установлен дефолтный");
            }
        }

        private readonly HttpClient _httpClient;
        private readonly ILogger<LlmService> _logger;
        private readonly string? _endpoint;
        private readonly string? _modelName;
        private readonly string? _systemPromt; 

        public async IAsyncEnumerable<string> ExecuteLlmAsync(List<object> history)
        {
            if(history == null || history.Count == 0) yield break;

            //data for API
            var messagesPayload = new List<object>
            {
                new { role = "system", content = _systemPromt }
            };

            messagesPayload.AddRange(history);

            var payload = new
            {
                model = _modelName,
                messages = messagesPayload,
                stream = true,
                max_tokens = 15,
                temperature = 0.2
            };
            //HTTP requests
            var request = new HttpRequestMessage(HttpMethod.Post, _endpoint);
            request.Content = JsonContent.Create(payload);

            using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);

            if (!response.IsSuccessStatusCode)
            {
                string errorLog = await response.Content.ReadAsStringAsync();
                _logger.Log(LogLevel.Error, $"[LlmService] {errorLog}");
                yield break;
            }

            using var stream = await response.Content.ReadAsStreamAsync();
            using var reader  = new StreamReader(stream);

            while(!reader.EndOfStream)
            {
                var line = await reader.ReadLineAsync();

                if (string.IsNullOrWhiteSpace(line)) continue;
                if (line.StartsWith("data: ")) line = line.Substring(6);
                if (line.Trim() == "[DATA]") break;

                string? textToYield = null;

                try
                {
                    using var doc = JsonDocument.Parse(line);
                    var root = doc.RootElement;

                    if (root.TryGetProperty("choices", out var choices) &&
                        choices.GetArrayLength() > 0)
                    {
                        var firstChoice = choices[0];
                        if (firstChoice.TryGetProperty("delta", out var delta) &&
                            delta.TryGetProperty("content", out var content))
                        {
                            var text = content.GetString();
                            if (!string.IsNullOrEmpty(text))
                            {
                                textToYield = text; 
                            }
                        }

                        if (firstChoice.TryGetProperty("finish_reason", out var finishReason) &&
                            finishReason.ValueKind != JsonValueKind.Null)
                        {
                            break;
                        }
                    }
                }
                catch (JsonException ex) 
                {
                    _logger.LogWarning($"[LlmService] Ошибка парсинга JSON: {ex.Message}. Строка: {line}");
                    continue; 
                }

                if (!string.IsNullOrWhiteSpace(textToYield)) yield return textToYield;

            }
        }
    }
}
