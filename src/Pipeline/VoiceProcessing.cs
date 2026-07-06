using DiscordVoiceBotMark.src.Orchestration;

namespace DiscordVoiceBotMark.src.Pipeline
{
    internal sealed class VoiceProcessing
    {
        public VoiceProcessing(ISpeechToTextService speechToTextService, IVoiceOutputService voiceOutputService,
            ILlmService llmService, ITextToSpeechService textToSpeechService) 
        { 
            _llmService = llmService;
            _speechToTextService = speechToTextService;
            _textToSpeechService = textToSpeechService;
            _voiceOutputService = voiceOutputService;
        }

        private readonly ISpeechToTextService _speechToTextService;
        private readonly ILlmService _llmService;
        private readonly ITextToSpeechService _textToSpeechService;
        private readonly IVoiceOutputService _voiceOutputService;

        public async Task ExecutePipelineAsync(ulong userId, byte[] inputAudio)
        {
            var userText = await _speechToTextService.ExecuteSpeechToTextAsync(inputAudio);
            if (string.IsNullOrWhiteSpace(userText)) return;

            var aiAnswer = await _llmService.ExecuteLlmAsync(userText);
            if(string.IsNullOrWhiteSpace(aiAnswer)) return;

            var outputAudio = await _textToSpeechService.ExecuteTextToSpeechAsync(aiAnswer);
            if(outputAudio == null || outputAudio.Length == 0) return;

            await _voiceOutputService.PlayAudioAsync(userId, outputAudio);
        }
    }
}
