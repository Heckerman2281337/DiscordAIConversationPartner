using Discord.Audio;
using DiscordVoiceBotMark.src.Conversion;
using DiscordVoiceBotMark.src.Voice;
using Microsoft.Extensions.Logging;

namespace DiscordVoiceBotMark.src.Pipeline
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
            if(session == null)
            {
                _logger.Log(LogLevel.Error, $"[VoiceOutputService] нет сессии для юзера: {userId}");
                return;
            }

            var guildId = session.GuildId;

            _sessionManager.TryGetAudioOutputClient(guildId, out var outputStream);
            if(outputStream == null)
            {
                _logger.Log(LogLevel.Error, $"[VoiceOutputService] output stream null для гильдии: {guildId}");
                return;
            }

            var stereo = await _converter.ConvertToStereoAsync(data);

            await outputStream.WriteAsync(stereo);
            await outputStream.FlushAsync();
        }
    }
}