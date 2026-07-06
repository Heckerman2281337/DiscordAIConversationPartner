using Discord.Audio;
using DiscordVoiceBotMark.src.Voice;
using Microsoft.Extensions.Logging;

namespace DiscordVoiceBotMark.src.Pipeline
{
    internal sealed class VoiceOutputService : IVoiceOutputService
    {
        public VoiceOutputService(IVoiceSessionManager sessionManager, ILogger<VoiceOutputService> logger)
        {
            _sessionManager = sessionManager;
            _logger = logger;
        }

        private readonly ILogger<VoiceOutputService> _logger;
        private readonly IVoiceSessionManager _sessionManager;

        public async Task PlayAudioAsync(ulong userId, byte[] data)
        {
            await Task.CompletedTask;
        }
    }
}