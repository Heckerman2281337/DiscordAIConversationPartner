
namespace DiscordVoiceBotMark.src.Pipeline
{
    public interface ISpeechToTextService
    {
        public Task<string?> ExecuteSpeechToTextAsync(byte[] data);
    }
    public interface ILlmService
    {
        public Task<string?> ExecuteLlmAsync(List<object> history);
    }
    public interface ITextToSpeechService
    {
        public Task<byte[]?> ExecuteTextToSpeechAsync(string aiAnswer);
    }
    public interface IVoiceOutputService
    {
        public Task PlayAudioAsync(ulong userId, byte[] data);
    }
}
