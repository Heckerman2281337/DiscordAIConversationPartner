using Microsoft.Extensions.Logging;
using Discord.Audio;
using Discord.WebSocket;

namespace DiscordVoiceBotMark.src.Voice
{
    internal interface IVoiceCaptureService
    {
        public Task StartListeningAsync(IAudioClient audioClient);
        public Task StopListeningAsync(IAudioClient audioClient);
    }
   
    internal sealed class VoiceCaptureService : IVoiceCaptureService
    {
        public VoiceCaptureService
            (IVoiceSessionManager sessionManager, ILogger<VoiceCaptureService> logger,
            IVoiceActivityDetector voiceDetector, DiscordSocketClient discordClient) 
        { 
            _sessionManager = sessionManager;
            _logger = logger;
            _voiceDetector = voiceDetector;
            _discordClient = discordClient;
        }

        private readonly ILogger<VoiceCaptureService> _logger;
        private readonly IVoiceSessionManager _sessionManager;
        private readonly IVoiceActivityDetector _voiceDetector;
        private readonly DiscordSocketClient _discordClient;

        public Task StartListeningAsync(IAudioClient audioClient)
        {
            audioClient.StreamCreated += OnStreamCreatedAsync;
            audioClient.StreamDestroyed += OnStreamDestroyedAsync;
            audioClient.ClientDisconnected += OnClientDisconnectedAsync;

            return Task.CompletedTask;
        }

        public Task StopListeningAsync(IAudioClient audioClient)
        {
            audioClient.StreamCreated -= OnStreamCreatedAsync;
            audioClient.StreamDestroyed -= OnStreamDestroyedAsync;
            audioClient.ClientDisconnected -= OnClientDisconnectedAsync;

            return Task.CompletedTask;
        }
        //if user starts talking
        private Task OnStreamCreatedAsync(ulong userId, AudioInStream stream)
        {
            var guildUser = _discordClient.Guilds
                .Select(g => g.GetUser(userId))
                .FirstOrDefault(u => u?.VoiceChannel != null);

            ulong channelId = guildUser?.VoiceChannel?.Id ?? 0;

            var session = _sessionManager.GetOrCreate(userId, channelId);

            _logger.Log(LogLevel.Information, $"[VoiceCaptureService] session: {session} " +
                $"was created for userId: {userId}");

            session.CurrentStreamCts = CancellationTokenSource.CreateLinkedTokenSource(session.ReadLoopCts.Token);

            _ = ReadAudioLoopAsync(session, stream, session.CurrentStreamCts.Token);
            if (!session.IsMonitored)
            {
                session.IsMonitored = true;
                _voiceDetector.StartChecking(session);
            }

            return Task.CompletedTask;
        }
        //if user mute himself
        private Task OnStreamDestroyedAsync(ulong userId)
        {   
            _sessionManager.TryGetByUserId(userId, out var session);

            if(session == null)
            {
                _logger.Log(LogLevel.Debug, $"[VoiceCaptureService] session is null");
                return Task.CompletedTask;
            }

            session.CurrentStreamCts?.Cancel();
            session.CurrentStreamCts?.Dispose();
            return Task.CompletedTask;  
        }
        //if user disconnecting
        private Task OnClientDisconnectedAsync(ulong userId)
        {
            _sessionManager.Remove(userId);
            _logger.Log(LogLevel.Information, $"[VoiceCaptureService] removed session for userId: {userId}");
            return Task.CompletedTask;
        }
        
        private async Task ReadAudioLoopAsync
            (UserVoiceSession voiceSession, AudioInStream stream, CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var frame = await stream.ReadFrameAsync(cancellationToken);
                    if (frame.Missed) continue;
                    await voiceSession.OpusFrames.Writer.WriteAsync(frame.Payload, cancellationToken);
                    voiceSession.LastPackageUTC = DateTime.UtcNow;
                    voiceSession.IsSpeaking = true;
                }
                catch (OperationCanceledException)
                {
                    _logger.Log(LogLevel.Information, $"Сancelled connection with userId: {voiceSession.UserId}");
                    return; 
                }
                catch (Exception ex)
                {
                    _logger.Log(LogLevel.Error, $"Error {ex.Message} in ReadAuidioLoopAsync");
                    await Task.Delay(5000, cancellationToken);
                }
            }
        }
    }
}
