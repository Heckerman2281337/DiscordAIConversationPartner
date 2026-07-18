using DiscordVoiceBotMark.Voice;
using Microsoft.Extensions.Logging;

namespace DiscordVoiceBotMark.Orchestration
{

    internal sealed class UtteranceCollector
    {
        public UtteranceCollector(ILogger<UtteranceCollector> logger,
            IVoiceSessionManager sessionManager, IVoiceActivityDetector voiceDetector)
        {
            _logger = logger;
            _voiceActivityDetector= voiceDetector;
            _sessionManager= sessionManager;

            _voiceActivityDetector.SpeechEnded += OnSpeechEnded;
        }


        private readonly IVoiceActivityDetector _voiceActivityDetector;
        private readonly IVoiceSessionManager _sessionManager;
        private readonly ILogger<UtteranceCollector> _logger;

        public event Func<ulong, ulong, byte[], Task>? TalkCollected;

        private async void OnSpeechEnded(ulong userId)
        {
            _sessionManager.TryGetByUserId(userId, out var session);

            if (session == null)
            {
                _logger.Log(LogLevel.Debug, $"[UtteranceCollector] сессия у {userId} не найдена");
                return;
            }

            try
            {
                using var memoryStream = new MemoryStream();

                short[] pcm = new short[1920]; // decoding goes by this formula: 48k * 0.02 sec * 2
                                               // 48k - standart discord frequency, 0.02 sec - standart discord frame, 2 - stereo 

                while (session.OpusFrames.Reader.TryRead(out var frame))
                {
                    await memoryStream.WriteAsync(frame);
                  //  _logger.Log(LogLevel.Information, $"[UtteranceCollector] в списке: {memoryStream.Length} элементов");
                }

                if (memoryStream.Length == 0) return;

                await ((TalkCollected?.Invoke(session.UserId, session.GuildId, memoryStream.ToArray())) ?? Task.CompletedTask);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[UtteranceCollector] Ошибка при кодировании аудио для пользователя {userId}");
            }
        }
    }
}
