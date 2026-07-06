using Discord;
using Discord.WebSocket;
using DiscordVoiceBotMark.src.Discord;
using DiscordVoiceBotMark.src.Orchestration;
using DiscordVoiceBotMark.src.Voice;
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

        public static async Task UseUtteranceCollectorAsync(this IHost host)
        {
            var utteranceCollector = host.Services.GetRequiredService<UtteranceCollector>();
            
        }
    }
}
