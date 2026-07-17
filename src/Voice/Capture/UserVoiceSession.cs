using System.Threading.Channels;

namespace DiscordVoiceBotMark.src.Voice
{
    //saves state of each user in voice channel
    public sealed class UserVoiceSession : IDisposable
    {
        public UserVoiceSession(ulong userId, ulong guildId)
        {
            UserId = userId;
            GuildId = guildId;
        }

        public ulong UserId { get; }
        public ulong GuildId { get; set; }
        public DateTime LastPackageUTC { get; set; } = DateTime.UtcNow;
        public bool IsSpeaking { get; set; } = false;
        public bool IsMonitored { get; set; } = false;


        public CancellationTokenSource? ProccessingCts { get; set;}
        public CancellationTokenSource ReadLoopCts { get; } = new(); 
        public CancellationTokenSource? CurrentStreamCts { get; set;}

        

        //Channel to avoid thread-like errors
        public Channel<byte[]> OpusFrames { get; } = Channel.CreateUnbounded<byte[]>(
            new UnboundedChannelOptions
            {
                SingleReader = true,
                SingleWriter = true,
            });

        //Dispose when user leaves channel
        public void Dispose()
        {
            CancelSafely(ReadLoopCts);
            CancelSafely(ProccessingCts);
            CancelSafely(CurrentStreamCts);

            DisposeSafely(ReadLoopCts);
            DisposeSafely(ProccessingCts);
            DisposeSafely(CurrentStreamCts);

            OpusFrames.Writer.TryComplete();
        }

        private void CancelSafely(CancellationTokenSource? cts)
        {
            try { cts?.Cancel(); } catch (ObjectDisposedException) { }
        }

        private void DisposeSafely(CancellationTokenSource? cts)
        {
            try { cts?.Dispose(); } catch (ObjectDisposedException) { }
        }
    }
}
