using System.Diagnostics;

namespace DiscordVoiceBotMark.src.Conversion
{

    public interface IEncoder
    {
        public Task<byte[]> ConvertPcmAsync(List<byte[]> pcm);
    }

    internal sealed class Mp3Encoder : IEncoder
    {
        public async Task<byte[]> ConvertPcmAsync(List<byte[]> pcm)
        {
            if (pcm == null || pcm.Count == 0) return Array.Empty<byte>();

            var startInfo = new ProcessStartInfo()
            {
                FileName = "ffmpeg",
                Arguments = "-f s16le -ar 48000 -ac 2 -i pipe:0 -codec:a libmp3lame -b:a 128k -f mp3 pipe:1",
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

                foreach (var chunk in pcm)
                {
                    await stdin.WriteAsync(chunk, 0, chunk.Length);
                }

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
            }

            return await readTask;
        }
    }
}
