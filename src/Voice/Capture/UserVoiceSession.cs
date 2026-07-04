using System.Threading.Channels;


namespace DiscordVoiceBotMark.src.Voice.Capture
{
    //saves state of each user in voice channel
    internal sealed class UserVoiceSession : IDisposable
    {
        public UserVoiceSession(ulong userId)
        {
            UserId = userId;
        }

        public ulong UserId { get; }
        public DateTime LastPackageUTC { get; set; } = DateTime.UtcNow;
        public bool IsSpeaking { get; set; }

        public CancellationTokenSource? ProccessingCts { get; set;}
        public CancellationTokenSource ReadLoopCts { get; } = new();

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
            ReadLoopCts.Cancel();
            ReadLoopCts.Dispose();
            ProccessingCts?.Cancel();
            ProccessingCts?.Dispose();
            OpusFrames.Writer.TryComplete();
        }
    }
}
