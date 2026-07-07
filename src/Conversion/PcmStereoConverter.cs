using Microsoft.Extensions.Logging;
using System.Diagnostics;


namespace DiscordVoiceBotMark.src.Conversion
{
    public interface IPcmStereoConverter
    {
        public Task<byte[]> ConvertToStereoAsync(byte[] data);
    }
    internal sealed class PcmStereoConverter : IPcmStereoConverter
    {
        public PcmStereoConverter(ILogger<PcmStereoConverter> logger)
        {
            _logger = logger;
        }

        private readonly ILogger<PcmStereoConverter> _logger;
        public async Task<byte[]> ConvertToStereoAsync(byte[] monoPcm)
        {

            if (monoPcm == null || monoPcm.Length == 0) return Array.Empty<byte>();

            var startInfo = new ProcessStartInfo()
            {
                FileName = "ffmpeg",
                Arguments = "-f s16le -ar 48000 -ac 1 -i pipe:0 -f s16le -ar 48000 -ac 2 pipe:1",
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

                await stdin.WriteAsync(monoPcm, 0, monoPcm.Length);

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
                _logger.Log(LogLevel.Error, $"[PcmStereoConverter] {errorLog}");
            }

            return await readTask;
        }
    }
}
