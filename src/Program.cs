using Microsoft.Extensions.Hosting;
using Discord.WebSocket;
using Discord;
using Microsoft.Extensions.DependencyInjection;
using DotNetEnv;
using DiscordVoiceBotMark.src.Config;

namespace DiscordVoiceBotMark.src.Voice
{
    internal class Program
    {
        static async Task Main()
        {
            Env.Load();
            var builder = Host.CreateApplicationBuilder();

            var discordConfig = new DiscordSocketConfig
            {
                GatewayIntents = GatewayIntents.Guilds |
                                 GatewayIntents.GuildMessages |
                                 GatewayIntents.GuildVoiceStates |
                                 GatewayIntents.MessageContent,
                LogLevel = LogSeverity.Info,
                EnableVoiceDaveEncryption = true,
            };

            builder.Services.AddSingleton(new DiscordSocketClient(discordConfig));

            builder.Services.AddVoiceServices();

            var host = builder.Build();
            await host.UseVoiceHandlersAsync();
            await host.UseUtteranceCollectorAsync();

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
                Console.WriteLine("[Program.cs] Не удалось найти переменную окружения BOT_TOKEN");
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
