using Microsoft.Extensions.Hosting;
using Discord.WebSocket;
using Discord;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using DiscordVoiceBotMark.src.Voice.Capture;
namespace DiscordVoiceBotMark.src
{
    internal class Program
    {
        static async Task Main()
        {
            var builder = Host.CreateApplicationBuilder();

            var discordConfig = new DiscordSocketConfig
            {
                GatewayIntents = GatewayIntents.Guilds |
                                 GatewayIntents.GuildMessages |
                                 GatewayIntents.GuildVoiceStates |
                                 GatewayIntents.MessageContent,
                LogLevel = LogSeverity.Info
            };

            builder.Services.AddSingleton(new DiscordSocketClient(discordConfig));
            builder.Services.AddSingleton<IVoiceSessionManager, VoiceSessionManager>();

            var host = builder.Build();
            var client = host.Services.GetRequiredService<DiscordSocketClient>();

            client.Log += LogAsync;

            string token = Environment.GetEnvironmentVariable("BOT_TOKEN")!; // чтобы компилятор не ругался

            if(!string.IsNullOrEmpty(token))
            {
                await client.LoginAsync(TokenType.Bot, token);
                await client.StartAsync();
            }
            else
            {
                Console.WriteLine("[Program.cs 39] Не удалось найти переменную окружения BOT_TOKEN");
            }

            await host.RunAsync();

            Task LogAsync(LogMessage message)
            {
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] [{message.Source}] {message.Message}");
                return Task.CompletedTask;
            }
        }
    }
}
