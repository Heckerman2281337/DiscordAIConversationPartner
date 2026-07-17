using DiscordVoiceBotMark.src.Conversion;
using DiscordVoiceBotMark.src.Discord;
using DiscordVoiceBotMark.src.Orchestration;
using DiscordVoiceBotMark.src.Pipeline;
using DiscordVoiceBotMark.src.Voice;
using Microsoft.Extensions.DependencyInjection;
using Whisper.net;

namespace DiscordVoiceBotMark.src.Config
{
    internal static class VoiceExtensions
    {
        public static string WhisperDownload()
        {
            string modelFolder = Path.Combine(AppContext.BaseDirectory, "Models");
            string modelPath = Path.Combine(modelFolder, "ggml-base.bin");
            string tempPath = modelPath + ".tmp";

            // if whisper model is ok
            if (File.Exists(modelPath))
            {
                long actualLength = new FileInfo(modelPath).Length;
                Console.WriteLine("[Whisper] Модель найдена локально, инициализирую фабрику.");
                Console.WriteLine($"[Debug] Файл обнаружен! Путь: {modelPath}");
                Console.WriteLine($"[Debug] Реальный размер файла на диске: {actualLength} байт");

                if (actualLength > 135_000_000)
                {
                    Console.WriteLine("[Whisper] Модель найдена локально, инициализирую фабрику.");
                    return modelPath;
                }

                Console.WriteLine($"[Debug] Размер {actualLength} меньше лимита 135_000_000. Файл признан битым.");
            }

            // if whisper model is corrupted
            if (File.Exists(modelPath)) File.Delete(modelPath);
            if (File.Exists(tempPath)) File.Delete(tempPath);

            Console.WriteLine("[Whisper] Модель не найдена или повреждена. Начинаю скачивание (141 MB)...");
            Directory.CreateDirectory(modelFolder);

            using var httpClient = new HttpClient();
            string downloadUrl = "https://huggingface.co/ggerganov/whisper.cpp/resolve/main/ggml-base.bin";

            try
            {
                using var response = httpClient.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead).GetAwaiter().GetResult();
                response.EnsureSuccessStatusCode();

                using (var fs = new FileStream(tempPath, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    response.Content.CopyToAsync(fs).GetAwaiter().GetResult();
                }

                File.Move(tempPath, modelPath);
                Console.WriteLine("[Whisper] Модель успешно скачана и сохранена!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Error] Критическая ошибка при скачивании модели Whisper: {ex.Message}");

                if (File.Exists(tempPath)) File.Delete(tempPath);
                throw;
            }

            return modelPath;
        }

        public static IServiceCollection AddVoiceServices(this IServiceCollection services)
        {
            var modelPath = WhisperDownload();

            services.AddSingleton<IVoiceSessionManager, VoiceSessionManager>();
            services.AddSingleton<IVoiceChannelController, VoiceChannelController>();
            services.AddSingleton<IVoiceCaptureService, VoiceCaptureService>();
            services.AddSingleton<IVoiceActivityDetector, VoiceActivityDetector>();
            services.AddSingleton<IChatHistoryManager, ChatHistoryManager>();
            services.AddSingleton<IPcmStereoConverter, PcmStereoConverter>();
            services.AddSingleton<IWavConverter, WavConverter>();

            services.AddSingleton<ISpeechToTextService, SpeechToTextService>();

            services.AddSingleton<WhisperFactory>(sp =>
            {
                return WhisperFactory.FromPath(modelPath);
            });

            services.AddHttpClient<ILlmService, LlmService>();
            services.AddHttpClient<ITextToSpeechService, XttsService>();
            services.AddSingleton<IVoiceOutputService, VoiceOutputService>();


            services.AddSingleton<VoiceProcessing>();
            services.AddSingleton<UtteranceCollector>();


            return services;
        }
    }
}
