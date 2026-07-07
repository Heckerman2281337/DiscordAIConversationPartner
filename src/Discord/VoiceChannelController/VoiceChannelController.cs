using Discord;
using Discord.Audio;
using Discord.WebSocket;
using DiscordVoiceBotMark.src.Voice;
using Microsoft.Extensions.Logging;

namespace DiscordVoiceBotMark.src.Discord
{

    internal interface IVoiceChannelController
    {
        public Task RegisterHandlersAsync(DiscordSocketClient client);
        public Task UnregisterHandlersAsync(DiscordSocketClient client);
    }

    internal sealed class VoiceChannelController : IVoiceChannelController
    {
        private readonly List<string> randomLeaveBotMessage = new List<string>
        {
            "Оййй конфа... отдохнуть от вас надо",
            "Я в тильте",
            "Оооо понятно ребят, я пошёл"
        };

        public VoiceChannelController(IVoiceCaptureService service, IVoiceSessionManager sessionManager,
            ILogger<VoiceChannelController> logger)
        { 
            _service = service;
            _logger = logger;
            _sessionManager = sessionManager;
        }

        private readonly ILogger<VoiceChannelController> _logger;
        private readonly IVoiceCaptureService _service;
        private readonly IVoiceSessionManager _sessionManager;

        public Task RegisterHandlersAsync(DiscordSocketClient client)
        {
            client.MessageReceived += OnMessageRecieved;

            return Task.CompletedTask;
        }

        public Task UnregisterHandlersAsync(DiscordSocketClient client)
        {
            client.MessageReceived -= OnMessageRecieved;
            return Task.CompletedTask;
        }

        private Task OnMessageRecieved(SocketMessage message)
        {
            if (message.Author.IsBot) return Task.CompletedTask;
            if (message.Content.StartsWith("!leave", StringComparison.OrdinalIgnoreCase)) _ = LeaveVoiceChannelAsync(message);
            if (message.Content.StartsWith("!join", StringComparison.OrdinalIgnoreCase)) _ = JoinVoiceChannelAsync(message);

            return Task.CompletedTask;
        }

        private async Task LeaveVoiceChannelAsync(SocketMessage message)
        {
            var textChannel = message.Channel as SocketTextChannel;
            if (textChannel == null) return;

            var guild = textChannel.Guild;
            var botVoiceChannel = guild.CurrentUser.VoiceChannel;

            if (botVoiceChannel != null)
            {
                try
                {
                    _sessionManager.ClearGuildSession(guild.Id);

                    await botVoiceChannel.DisconnectAsync();

                    int randomIndex = Random.Shared.Next(randomLeaveBotMessage.Count);
                    await textChannel.SendMessageAsync(randomLeaveBotMessage[randomIndex]);
                }
                catch (Exception ex)
                {
                    _logger.Log(LogLevel.Error, $"[VoiceChannelController] Error while leaving: {ex.Message}");
                }
            }
        }

        private async Task JoinVoiceChannelAsync(SocketMessage message)
        {
            try
            {
                var author = message.Author as IGuildUser;
                if (author == null)
                {
                    _logger.Log(LogLevel.Error, "[VoiceChannelController] Cant connect to user. User is null");
                    return;
                }

                if (author.VoiceChannel == null)
                {
                    _logger.Log(LogLevel.Error, "[VoiceChannelController] Cant connect to user. User is not in voice channel");
                    return;
                }

                var guildId = author.Guild.Id;
                //input audio
                var audioClient = await author.VoiceChannel.ConnectAsync();
                _sessionManager.SetAudioInputClient(guildId, audioClient);

                //output audio
                var outStream = audioClient.CreatePCMStream(AudioApplication.Voice);
                _sessionManager.SetAudioOutputClient(guildId, outStream);

                await _service.StartListeningAsync(audioClient);
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, $"[VoiceChannelController] Cant connect to user. {ex.Message}");
                return;
            }
        }
    }
}
