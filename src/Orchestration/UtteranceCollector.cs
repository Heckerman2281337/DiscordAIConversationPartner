using Discord.Audio;
using DiscordVoiceBotMark.src.Conversion;
using DiscordVoiceBotMark.src.Voice;
using Microsoft.Extensions.Logging;

namespace DiscordVoiceBotMark.src.Orchestration
{

    internal sealed class UtteranceCollector
    {
        public UtteranceCollector(ILogger<UtteranceCollector> logger, IEncoder encoder,
            IVoiceSessionManager sessionManager, IVoiceActivityDetector voiceDetector)
        {
            _logger = logger;
            _voiceActivityDetector= voiceDetector;
            _sessionManager= sessionManager;
            _encoder = encoder;

            _voiceActivityDetector.SpeechEnded += OnSpeechEnded;
        }


        private readonly IVoiceActivityDetector _voiceActivityDetector;
        private readonly IVoiceSessionManager _sessionManager;
        private readonly ILogger<UtteranceCollector> _logger;
        private readonly IEncoder _encoder;

        public event Func<ulong, ulong, byte[], Task>? TalkCollected;

        private async void OnSpeechEnded(ulong userId)
        {
            _sessionManager.TryGetByUserId(userId, out var session);

            if (session == null)
            {
                _logger.Log(LogLevel.Debug, $"[UtteranceCollector] сессия у {userId} не найдена");
                return;
            }

            List<byte[]> userTalk = new();

            short[] pcm = new short[1920]; // decoding goes by this formula: 48k * 0.02 sec * 2
                                           // 48k - standart discord frequency, 0.02 sec - standart discord frame, 2 - stereo 

            while (session.OpusFrames.Reader.TryRead(out var frame))
            {
                var decodedSamples = session.OpusDecoder.Decode(frame, pcm, 960); 

                byte[] byteBuffer = new byte[3840]; // creating new byte buffer for sample
                Buffer.BlockCopy(pcm, 0, byteBuffer, 0, 3840);

                userTalk.Add(byteBuffer);
                _logger.Log(LogLevel.Information, $"[UtteranceCollector] в списке: {userTalk.Count} элементов");
            }

            if (userTalk.Count == 0) return;

            try
            {
                _logger.LogInformation($"[UtteranceCollector] Начинаем сжатие {userTalk.Count} фреймов в MP3");
                byte[] audio = await _encoder.ConvertPcmAsync(userTalk);
                _logger.LogInformation($"[UtteranceCollector] Сжатие завершено. Получено {audio.Length} байт MP3.");

                if (audio != null) await ((TalkCollected?.Invoke(session.UserId, session.ChannelId, audio)) ?? Task.CompletedTask);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[UtteranceCollector] Ошибка при кодировании аудио для пользователя {userId}");
            }
        }
    }
}
