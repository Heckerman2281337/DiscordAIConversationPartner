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

            var wakeWordsRaw = Environment.GetEnvironmentVariable("WAKE_WORDS") ?? "";

            _wakeWords = wakeWordsRaw.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList();

            if (!_wakeWords.Any())
                _logger.LogWarning("Список Wake words пуст");
        }

        private readonly ILogger<SpeechToTextService> _logger;
        private readonly WhisperFactory _factory;
        private readonly IWavConverter _wavConverter;
        private readonly List<string> _wakeWords;
        private static readonly string[] hallucinations = new[]
        {
            "субтитры", "продолжение следует", "спасибо за просмотр",
            "подписывайтесь на канал", "поставьте лайк", "копирование запрещено",
            "перевод", "редактор", "озвучка"
        };

        public async Task<string?> ExecuteSpeechToTextAsync(byte[] data)
        {
            if (data == null || data.Length == 0) return null;

            byte[] wavData = await _wavConverter.ConvertAsync(data);
            if(wavData.Length == 0) return null;

            using var wavStream = new MemoryStream(wavData);

            using var processor = _factory.CreateBuilder()
                .WithLanguage("ru")
                .WithGreedySamplingStrategy()
                .WithEntropyThreshold(2.4f)
                .Build();
            
            StringBuilder builder = new();
            await foreach (var wavSegment in processor.ProcessAsync(wavStream))
            {
                var cleanSegment = CleanAndFilterSegment(wavSegment.Text);
                if (!string.IsNullOrEmpty(cleanSegment)) builder.Append(cleanSegment).Append(' ');
            }
            
            var text = builder.ToString().Trim();
           
            bool hasWakeWord = _wakeWords.Any(name => text.Contains(name, StringComparison.OrdinalIgnoreCase));
            if (!hasWakeWord) return null;

            return text;
        }

        private string? CleanAndFilterSegment(string segment)
        {
            if (string.IsNullOrWhiteSpace(segment)) return null;

            segment = Regex.Replace(segment, @"\[.*?\]|\(.*?\)", "").Trim();

            foreach (var hallucination in hallucinations)
            {
                if (segment.Contains(hallucination, StringComparison.OrdinalIgnoreCase))
                {
                    if (segment.Length <= hallucination.Length + 15)
                    {
                        _logger.LogDebug("Отфильтрован сегмент-галлюцинация Whisper: '{Segment}'", segment);
                        return null;
                    }
                }
            }

            if (segment.Length < 2 || !segment.Any(char.IsLetterOrDigit)) return null;

            return segment;
        }
    }
}
