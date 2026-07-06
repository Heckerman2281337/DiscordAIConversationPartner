using Discord.Rest;
using DiscordVoiceBotMark.src.Orchestration;

namespace DiscordVoiceBotMark.src.Pipeline
{
    internal sealed class VoiceProcessing
    {
        public VoiceProcessing(ISpeechToTextService speechToTextService, IVoiceOutputService voiceOutputService,
            ILlmService llmService, ITextToSpeechService textToSpeechService, IChatHistoryManager chatHistoryManager) 
        { 
            _llmService = llmService;
            _speechToTextService = speechToTextService;
            _textToSpeechService = textToSpeechService;
            _voiceOutputService = voiceOutputService;
            _chatHistoryManager = chatHistoryManager;
        }

        private readonly ISpeechToTextService _speechToTextService;
        private readonly ILlmService _llmService;
        private readonly ITextToSpeechService _textToSpeechService;
        private readonly IVoiceOutputService _voiceOutputService;
        private readonly IChatHistoryManager _chatHistoryManager;

        //From id to names
        private readonly Dictionary<ulong, string> _names = new()
        {
            {468700134008553472, "Шурик"},
            {301366361236570122, "Миша" },
            {522805439767904266, "Андрей"},
            {476665894182060042, "Марк" },
            {349152582528270347, "Саша" },
            {351760723493257217, "Рома" },
            {589711182290485279, "Бодя" },
            {270393513278046209, "Марк" },
            {360708126124670976, "Илья" },
            {831916416592379914, "Паша" },
            {762304407546494997, "Кэзбек" },
            {1180964704856846457, "Илья васдаф" }
        };


        public async Task ExecutePipelineAsync(ulong userId, ulong channelId, byte[] inputAudio)
        {
            var userText = await _speechToTextService.ExecuteSpeechToTextAsync(inputAudio);
            if (string.IsNullOrWhiteSpace(userText)) return;

            string username = _names.TryGetValue(userId, out var name) ? name : $"Юзер_{userId}";

            string formattedPromt = $"[{username}]: {userText}";
            _chatHistoryManager.AddMessage(channelId, "user", formattedPromt);

            var aiAnswer = await _llmService.ExecuteLlmAsync(_chatHistoryManager.GetHistory(channelId));
            if(string.IsNullOrWhiteSpace(aiAnswer)) return;

            _chatHistoryManager.AddMessage(channelId, "assistant", aiAnswer);

            var outputAudio = await _textToSpeechService.ExecuteTextToSpeechAsync(aiAnswer);
            if(outputAudio == null || outputAudio.Length == 0) return;

            await _voiceOutputService.PlayAudioAsync(userId, outputAudio);
        }
    }
}
