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
            {1180964704856846457, "Илья васдаф" }
        };


        public async Task ExecutePipelineAsync(ulong userId, ulong guildId, byte[] inputAudio)
        {
            var sw = Stopwatch.StartNew();


            var swStt = Stopwatch.StartNew();
            var userText = await _speechToTextService.ExecuteSpeechToTextAsync(inputAudio);
            swStt.Stop();
            _logger.LogInformation($"[Timing] STT занял: {swStt.ElapsedMilliseconds} мс");

            if (string.IsNullOrWhiteSpace(userText)) return;

            
            string username = _names.TryGetValue(userId, out var name) ? name : $"Юзер_{userId}";

            string formattedPromt = $"[{username}]: {userText}";
            _chatHistoryManager.AddMessage(guildId, "user", formattedPromt);
            _logger.LogInformation($"[STT] {username} сказал: {userText}");

            var swLlm = Stopwatch.StartNew();
            var aiAnswer = await _llmService.ExecuteLlmAsync(_chatHistoryManager.GetHistory(guildId));
            swLlm.Stop();
            _logger.LogInformation($"[LLM] ai ответил: {aiAnswer} мс");
            _logger.LogInformation($"[Timing] LLM занял: {swLlm.ElapsedMilliseconds} мс");
            if (string.IsNullOrWhiteSpace(aiAnswer)) return;


            _chatHistoryManager.AddMessage(guildId, "assistant", aiAnswer);

            var swTts = Stopwatch.StartNew();
            var outputAudio = await _textToSpeechService.ExecuteTextToSpeechAsync(aiAnswer);
            swTts.Stop();
            _logger.LogInformation($"[Timing] TTS занял: {swTts.ElapsedMilliseconds} мс");
            if (outputAudio == null || outputAudio.Length == 0) return;

            sw.Stop();
            _logger.LogInformation($"[Timing] Общее время от получения аудио до готовности к проигрыванию: {sw.ElapsedMilliseconds} мс");

            await _voiceOutputService.PlayAudioAsync(userId, outputAudio);
        }
    }
}
