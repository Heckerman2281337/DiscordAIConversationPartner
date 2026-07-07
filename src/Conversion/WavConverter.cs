using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace DiscordVoiceBotMark.src.Conversion
{

    public interface IWavConverter
    {
        public Task<byte[]> ConvertAsync(byte[] pcm);
    }

    internal sealed class WavConverter : IWavConverter
    {
        public WavConverter(ILogger<WavConverter> logger) 
        { 
            _logger = logger;
        }

        private readonly ILogger<WavConverter> _logger;

        public async Task<byte[]> ConvertAsync(byte[] pcm)
        {
            if (pcm == null || pcm.Length == 0) return Array.Empty<byte>();

            var startInfo = new ProcessStartInfo()
            {
                FileName = "ffmpeg",
                Arguments = "-f s16le -ar 48000 -ac 2 -i pipe:0 -ar 16000 -ac 1 -c:a pcm_s16le -f wav pipe:1",
                UseShellExecute = false,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
            };

            using var process = new Process { StartInfo = startInfo };
            process.Start();

            // We will make writer and reader to avoid deadlock
            var writeTask = Task.Run(async () =>
            {
                using var stdin = process.StandardInput.BaseStream;

                await stdin.WriteAsync(pcm, 0, pcm.Length);

                await stdin.FlushAsync();
            });

            var readTask = Task.Run(async () =>
            {
                using var memoryStream = new MemoryStream();
                await process.StandardOutput.BaseStream.CopyToAsync(memoryStream);
                return memoryStream.ToArray();
            });

            var errorTask = process.StandardError.ReadToEndAsync();

            await Task.WhenAll(writeTask, readTask);
            await process.WaitForExitAsync();

            if (process.ExitCode != 0)
            {
                var errorLog = await errorTask;
                _logger.Log(LogLevel.Error, $"[WavConverter] {errorLog}");
            }

            return await readTask;
        }
    }
}
