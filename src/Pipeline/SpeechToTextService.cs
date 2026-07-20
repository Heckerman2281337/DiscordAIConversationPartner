using Microsoft.Extensions.Logging;
using Whisper.net;
using DiscordVoiceBotMark.Conversion;
using System.Text;
using System.Text.RegularExpressions;

namespace DiscordVoiceBotMark.Pipeline
{
    internal sealed class SpeechToTextService : ISpeechToTextService
    {
        public SpeechToTextService(WhisperFactory factory, IWavConverter wavConverter, ILogger<SpeechToTextService> logger)
        {
            _logger = logger;
            _factory = factory;
            _wavConverter = wavConverter;
        }

        private readonly ILogger<SpeechToTextService> _logger;
        private readonly WhisperFactory _factory;
        private readonly IWavConverter _wavConverter;

        //Hardcode names for bot. Fix later

        private readonly List<string> _wakeWords = new()
        {
            "Марк",
            "Маркуха",
            "Мамытяу"
        };

        public async Task<string?> ExecuteSpeechToTextAsync(byte[] data)
        {
            if (data == null || data.Length == 0) return null;

            byte[] wavData = await _wavConverter.ConvertAsync(data);
            if(wavData.Length == 0) return null;

            using var wavStream = new MemoryStream(wavData);

            using var processor = _factory.CreateBuilder().WithLanguage("ru").Build();
            
            StringBuilder builder = new();
            await foreach (var wavSegment in processor.ProcessAsync(wavStream))
            {
                builder.Append(wavSegment.Text);
            }
            
            var text = builder.ToString().Trim();
            /*
            text = Regex.Replace(text, @"\[.*?\]|\(.*?\)", "").Trim();

            var hallucinations = new[] { "субтитры", "продолжение следует", "спасибо за просмотр" };
            
            if (hallucinations.Any(h => text.Contains(h, StringComparison.OrdinalIgnoreCase)))
            {
                return null;
            }

            if (string.IsNullOrWhiteSpace(text) || text.Length < 2)
            {
                return null;
            }
            */
            bool hasWakeWord = _wakeWords.Any(name => text.Contains(name, StringComparison.OrdinalIgnoreCase));
            
            if (!hasWakeWord) return null;

            return text;
        }
    }
}
