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
                return WhisperFactory.FromPath(Path.Combine(AppContext.BaseDirectory, "Models", "ggml-base.bin"));
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
