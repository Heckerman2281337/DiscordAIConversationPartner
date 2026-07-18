using Discord.Audio;
using DiscordVoiceBotMark.Conversion;
using DiscordVoiceBotMark.Voice;
using Microsoft.Extensions.Logging;

namespace DiscordVoiceBotMark.Pipeline
{
    internal sealed class VoiceOutputService : IVoiceOutputService
    {
        public VoiceOutputService(IVoiceSessionManager sessionManager, IPcmStereoConverter converter, 
            ILogger<VoiceOutputService> logger)
        {
            _sessionManager = sessionManager;
            _logger = logger;
            _converter = converter;
        }

        private readonly ILogger<VoiceOutputService> _logger;
        private readonly IVoiceSessionManager _sessionManager;
        private readonly IPcmStereoConverter _converter;

        public async Task PlayAudioAsync(ulong userId, byte[] data)
        {
            _sessionManager.TryGetByUserId(userId, out var session);
            if (session == null)
            {
                _logger.Log(LogLevel.Error, $"[VoiceOutputService] нет сессии для юзера: {userId}");
                return;
            }

            var guildId = session.GuildId;

            _sessionManager.TryGetAudioOutputClient(guildId, out var outputStream);
            if (outputStream == null)
            {
                _logger.Log(LogLevel.Error, $"[VoiceOutputService] output stream null для гильдии: {guildId}");
                return;
            }

            var stereo = await _converter.ConvertToStereoAsync(data);
            if (stereo == null || stereo.Length == 0) return;

            const int frameSize = 3840; // 20 ms

            try
            {
                for (int offset = 0; offset < stereo.Length; offset += frameSize)
                {
                    int bytesToWrite = Math.Min(frameSize, stereo.Length - offset);

                    await outputStream.WriteAsync(stereo, offset, bytesToWrite);
                }
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, $"[VoiceOutputService] Ошибка при отправке аудио в Discord: {ex.Message}");
            }
            finally
            {
                await outputStream.FlushAsync();
            }
        }
    }
}