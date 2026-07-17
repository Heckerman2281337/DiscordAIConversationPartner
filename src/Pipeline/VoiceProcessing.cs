using DiscordVoiceBotMark.src.Orchestration;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace DiscordVoiceBotMark.src.Pipeline
{
    internal sealed class VoiceProcessing
    {
        public VoiceProcessing(ILogger<VoiceProcessing> logger,
            ISpeechToTextService speechToTextService, IVoiceOutputService voiceOutputService,
            ILlmService llmService, ITextToSpeechService textToSpeechService, IChatHistoryManager chatHistoryManager) 
        { 
            _llmService = llmService;
            _speechToTextService = speechToTextService;
            _textToSpeechService = textToSpeechService;
            _voiceOutputService = voiceOutputService;
            _chatHistoryManager = chatHistoryManager;
            _logger = logger;

            _logger.LogCritical("!!! КОНСТРУКТОР VOICEPROCESSING ВЫЗВАН !!!");

        }

        private readonly ISpeechToTextService _speechToTextService;
        private readonly ILlmService _llmService;
        private readonly ITextToSpeechService _textToSpeechService;
        private readonly IVoiceOutputService _voiceOutputService;
        private readonly IChatHistoryManager _chatHistoryManager;
        private readonly ILogger<VoiceProcessing> _logger;
        //From id to names
        private readonly Dictionary<ulong, string> _names = new()
        {
            {468700134008553472, "Шурик"},
            {301366361236570122, "Миша" },
            {522805439767904266, "Андрей"},
            {476665894182060042, "Марк" },
            {349152582528270347, "Саша Зильбер" },
            {351760723493257217, "Рома" },
            {589711182290485279, "Бодя" },
            {270393513278046209, "Марк" },
            {360708126124670976, "Илья Лучов" },
            {831916416592379914, "Паша" },
            {762304407546494997, "Кэзбек" },
            {1180964704856846457, "Илья васдаф" },
            {358608717911949312, "Саша Марченко" }
        };


        public async Task ExecutePipelineAsync(ulong userId, ulong guildId, byte[] inputAudio)
        {
            try 
            {
                var sw = Stopwatch.StartNew();

                //STT
                var userText = await _speechToTextService.ExecuteSpeechToTextAsync(inputAudio);
                if (string.IsNullOrWhiteSpace(userText)) return;

                string username = _names.TryGetValue(userId, out var name) ? name : $"Юзер_{userId}";
                string formattedPromt = $"[{username}]: {userText}";

                _chatHistoryManager.AddMessage(guildId, "user", formattedPromt);
                _logger.LogInformation($"[STT] {username} сказал: {userText}");

                // LLM and TTS
                var history = _chatHistoryManager.GetHistory(guildId);
                await ProcessLlmStreamAndPlayAsync(history, userId, guildId);

                sw.Stop();
                _logger.LogInformation($"[Timing] Общее время цикла: {sw.ElapsedMilliseconds} мс");

            }
            catch (Exception ex)
            {
                _logger.LogCritical($"!!! КРИТИЧЕСКАЯ ОШИБКА В ПАЙПЛАЙНЕ: {ex.Message} \n {ex.StackTrace}");
            }
        }

        private async Task ProcessLlmStreamAndPlayAsync(List<object> history, ulong userId, ulong guildId)
        {
            _logger.LogInformation($"[DEBUG] ProcessLlmStreamAndPlayAsync");
            var sentenceBuffer = new System.Text.StringBuilder();
            var fullAiAnswer = new System.Text.StringBuilder(); 

            await foreach (var token in _llmService.ExecuteLlmAsync(history))
            {
                _logger.LogInformation($"[DEBUG] Токен: '{token}'");
                sentenceBuffer.Append(token);
                fullAiAnswer.Append(token);

                if (token.Contains('.') || token.Contains('!') || token.Contains('?') || token.Contains('\n'))
                {
                    var sentence = sentenceBuffer.ToString().Trim();

                    if (!string.IsNullOrWhiteSpace(sentence))
                    {
                        await ProcessAndPlaySentenceAsync(sentence, userId);
                    }
                    sentenceBuffer.Clear();
                }
            }

            if (sentenceBuffer.Length > 0)
            {
                await ProcessAndPlaySentenceAsync(sentenceBuffer.ToString().Trim(), userId);
            }

            _chatHistoryManager.AddMessage(guildId, "assistant", fullAiAnswer.ToString());
        }

        private async Task ProcessAndPlaySentenceAsync(string sentence, ulong userId)
        {
            _logger.LogInformation($"[TTS] Озвучка: {sentence}");
            var outputAudio = await _textToSpeechService.ExecuteTextToSpeechAsync(sentence);

            if (outputAudio != null && outputAudio.Length > 0)
            {
                _logger.LogInformation($"[TTS] Аудио успешно сгенерировано ({outputAudio.Length} байт), отправляю в Discord...");
                await _voiceOutputService.PlayAudioAsync(userId, outputAudio);
            }
        }
    }
}
