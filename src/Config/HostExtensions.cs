
using Discord;
using Discord.WebSocket;
using DiscordVoiceBotMark.src.Discord;
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
        }
    }
}
