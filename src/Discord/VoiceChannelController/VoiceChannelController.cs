using Discord;
using Discord.Audio;
using Discord.WebSocket;
using DiscordVoiceBotMark.src.Voice.Capture;
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

        public VoiceChannelController(IVoiceCaptureService service, ILogger<VoiceChannelController> logger)
        { 
            _service = service;
            _logger = logger;
        }

        private IAudioClient? _audioClient;
        private readonly ILogger<VoiceChannelController> _logger;
        private readonly IVoiceCaptureService _service;

        public Task RegisterHandlersAsync(DiscordSocketClient client)
        {
            client.MessageReceived += OnMessageRecieved;

            return Task.CompletedTask;
        }

        public Task UnregisterHandlersAsync(DiscordSocketClient client)
        {
            throw new NotImplementedException();
        }

        private Task OnMessageRecieved(SocketMessage message)
        {
            if (message.Author.IsBot) return Task.CompletedTask;
            if (message.Content.StartsWith("!leave")) _ = LeaveVoiceChannelAsync(message);
            if (message.Content.StartsWith("!join")) _ = JoinVoiceChannelAsync(message);

            return Task.CompletedTask;
        }

        private async Task LeaveVoiceChannelAsync(SocketMessage message)
        {
            var textChannel = message.Channel as SocketTextChannel;
            if (textChannel == null) return;    

            var guild = textChannel.Guild;
            var botVoiceChannel = guild.CurrentUser.VoiceChannel;
            int randomIndex = Random.Shared.Next(randomLeaveBotMessage.Count);

            if (botVoiceChannel != null)
            {
                await botVoiceChannel.DisconnectAsync();
                await textChannel.SendMessageAsync(randomLeaveBotMessage[randomIndex]);
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

                _audioClient = await author.VoiceChannel.ConnectAsync();

                await _service.StartListeningAsync(_audioClient);
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, $"[VoiceChannelController] Cant connect to user. {ex.Message}");
                return;
            }
        }
    }
}
