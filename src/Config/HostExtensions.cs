using Discord.WebSocket;
using DiscordVoiceBotMark.src.Discord;
using DiscordVoiceBotMark.src.Orchestration;
using DiscordVoiceBotMark.src.Pipeline;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace DiscordVoiceBotMark.src.Config
{
    internal static class HostExtensions
    {
        public static async Task UseVoiceHandlersAsync(this IHost host)
        {
            var client = host.Services.GetRequiredService<DiscordSocketClient>();
            
            var voiceChannelController = host.Services.GetRequiredService<IVoiceChannelController>();
            await voiceChannelController.RegisterHandlersAsync(client);
           // var test = host.Services.GetRequiredService<IVoiceActivityDetector>();
           // test.SpeechEnded += userId => Console.WriteLine($"[TEST] Речь юзера {userId} закончилась");
        }

        public static void UseVoicePipeline(this IHost host)
        {
            var collector = host.Services.GetRequiredService<UtteranceCollector>();
            var pipeline = host.Services.GetRequiredService<VoiceProcessing>();

            collector.TalkCollected += pipeline.ExecutePipelineAsync;
        }
    }
}
