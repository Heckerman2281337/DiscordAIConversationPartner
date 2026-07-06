
namespace DiscordVoiceBotMark.src.Voice
{
    public interface IVoiceActivityDetector
    {
        event Action<ulong> SpeechEnded;
        
        public void StartChecking(UserVoiceSession session);
    }

    internal sealed class VoiceActivityDetector : IVoiceActivityDetector
    {
        public event Action<ulong>? SpeechEnded;

        public void StartChecking(UserVoiceSession session)
        {
            _ = SilenceDetectionAsync(session);
        }

        private async Task SilenceDetectionAsync(UserVoiceSession session)
        {
            var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(200));

            while (await timer.WaitForNextTickAsync(session.ReadLoopCts.Token)) 
            {
                var lastPackageTime = DateTime.UtcNow - session.LastPackageUTC;

                if (lastPackageTime > TimeSpan.FromMilliseconds(1200) && session.IsSpeaking == true)
                {
                    SpeechEnded?.Invoke(session.UserId);
                    session.IsSpeaking = false;
                }
            }

        }
    }
}
