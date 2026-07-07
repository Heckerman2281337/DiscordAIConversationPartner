using System.Threading.Channels;
using Concentus;
using Concentus.Structs;

namespace DiscordVoiceBotMark.src.Voice
{
    //saves state of each user in voice channel
    public sealed class UserVoiceSession : IDisposable
    {
        public UserVoiceSession(ulong userId, ulong guildId)
        {
            UserId = userId;
            GuildId = guildId;
            OpusDecoder = OpusCodecFactory.CreateDecoder(48000, 2);
        }

        public ulong UserId { get; }
        public ulong GuildId { get; set; }
        public DateTime LastPackageUTC { get; set; } = DateTime.UtcNow;
        public bool IsSpeaking { get; set; } = false;
        public bool IsMonitored { get; set; } = false;

        public IOpusDecoder OpusDecoder { get; set; }

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
            ReadLoopCts.Cancel();
            ReadLoopCts.Dispose();

            ProccessingCts?.Cancel();
            ProccessingCts?.Dispose();

            CurrentStreamCts?.Cancel();
            CurrentStreamCts?.Dispose();

            OpusFrames.Writer.TryComplete();
        }
    }
}
