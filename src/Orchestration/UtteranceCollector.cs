using DiscordVoiceBotMark.src.Voice;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace DiscordVoiceBotMark.src.Orchestration
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

        private void OnSpeechEnded(ulong userId)
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
                var decoder = session.OpusDecoder;

                var decodedSamples = session.OpusDecoder.Decode(frame, pcm, 960); 

                byte[] byteBuffer = new byte[3840]; // creating new byte buffer for sample
                Buffer.BlockCopy(pcm, 0, byteBuffer, 0, 3840); // 

                userTalk.Add(byteBuffer);
                _logger.Log(LogLevel.Information, $"[UtteranceCollector] в списке: {userTalk.Count} элементов");
            }
        }

    }
}
