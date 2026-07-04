using Microsoft.Extensions.Logging;
using Discord.Audio;

namespace DiscordVoiceBotMark.src.Voice.Capture
{
    internal interface IVoiceCaptureService
    {
        public Task StartListeningAsync(IAudioClient audioClient);
        public Task StopListeningAsync(IAudioClient audioClient);
    }

    internal sealed class VoiceCaptureService : IVoiceCaptureService
    {
        public VoiceCaptureService(IVoiceSessionManager sessionManager, ILogger<VoiceCaptureService> logger) 
        { 
            _sessionManager = sessionManager;
            _logger = logger;
        }

        private readonly ILogger<VoiceCaptureService> _logger;
        private readonly IVoiceSessionManager _sessionManager;

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
            var session = _sessionManager.GetOrCreate(userId);

            _logger.Log(LogLevel.Information, $"[VoiceCaptureService] session: {session} " +
                $"was created for userId: {userId}");

            _ = ReadAudioLoopAsync(session, stream, session.ReadLoopCts.Token);
            return Task.CompletedTask;
        }
        //if user mute himself
        private Task OnStreamDestroyedAsync(ulong userId)
        {   

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
            (UserVoiceSession voiceSession,AudioInStream stream, CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var frame = await stream.ReadFrameAsync(cancellationToken);
                    if (frame.Missed) continue;
                    await voiceSession.OpusFrames.Writer.WriteAsync(frame.Payload, cancellationToken);
                    voiceSession.LastPackageUTC = DateTime.UtcNow;
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
