using DiscordVoiceBotMark.src.Conversion;
using DiscordVoiceBotMark.src.Discord;
using DiscordVoiceBotMark.src.Orchestration;
using DiscordVoiceBotMark.src.Pipeline;
using DiscordVoiceBotMark.src.Voice;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using Whisper.net;

namespace DiscordVoiceBotMark.src.Config
{
    internal static class VoiceExtensions
    {
        public static IServiceCollection AddVoiceServices(this IServiceCollection services)
        {
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
                string modelFolder = Path.Combine(AppContext.BaseDirectory, "Models");
                string modelPath = Path.Combine(modelFolder, "ggml-base.bin");

                if (!File.Exists(modelPath))
                {
                    Console.WriteLine("[Whisper] Модель ggml-base.bin не найдена. Начинаю скачивание (141 MB)...");
                    Directory.CreateDirectory(modelFolder);

                    using var httpClient = new HttpClient();
                    string downloadUrl = "https://huggingface.co/ggerganov/whisper.cpp/resolve/main/ggml-base.bin";

                    try
                    {
                        using var response = httpClient.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead).GetAwaiter().GetResult();
                        response.EnsureSuccessStatusCode();

                        using var fs = new FileStream(modelPath, FileMode.Create, FileAccess.Write, FileShare.None);
                        response.Content.CopyToAsync(fs).GetAwaiter().GetResult();

                        Console.WriteLine("[Whisper] Модель успешно скачана и сохранена!");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[Error] Критическая ошибка при скачивании модели Whisper: {ex.Message}");
                        throw;
                    }
                }
                else
                {
                    Console.WriteLine("[Whisper] Модель найдена локально, инициализирую фабрику.");
                }

                return WhisperFactory.FromPath(modelPath);
            });

            services.AddHttpClient<ILlmService, LlmService>();
            services.AddHttpClient<ITextToSpeechService, TextToSpeechService>();
            services.AddSingleton<IVoiceOutputService, VoiceOutputService>();


            services.AddSingleton<VoiceProcessing>();
            services.AddSingleton<UtteranceCollector>();


            return services;
        }
    }
}
