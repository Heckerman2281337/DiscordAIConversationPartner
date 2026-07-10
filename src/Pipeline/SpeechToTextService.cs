using Microsoft.Extensions.Logging;
using Whisper.net;
using NAudio.Wave;
using DiscordVoiceBotMark.src.Conversion;
using System.Text;

namespace DiscordVoiceBotMark.src.Pipeline
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

        public async Task<string?> ExecuteSpeechToTextAsync(byte[] data)
        {
            if (data == null || data.Length == 0) return null;

            byte[] wavData = await _wavConverter.ConvertAsync(data);

            if(wavData.Length == 0) return null;

            using var wavStream = new MemoryStream(wavData);

            var processor = _factory.CreateBuilder().WithLanguage("ru").Build();
            

            StringBuilder builder = new();
            
            await foreach (var wavSegment in processor.ProcessAsync(wavStream))
            {
                builder.Append(wavSegment.Text);
            }

            _logger.LogInformation($"user сказал: {builder.ToString()}");

            return builder.ToString().Trim();
        }
    }
}
