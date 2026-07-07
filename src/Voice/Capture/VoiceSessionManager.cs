using Discord.Audio;
using System.Collections.Concurrent;

namespace DiscordVoiceBotMark.src.Voice
{
    //For DI
    internal interface IVoiceSessionManager
    {
        public UserVoiceSession GetOrCreate(ulong userId, ulong channelId);
        public bool TryGetByUserId(ulong userId, out UserVoiceSession? session);
        public void Remove(ulong userId);
        public void SetAudioInputClient(ulong guildId, IAudioClient client);
        public bool TryGetAudioInputClient(ulong guildId, out IAudioClient? client);
        public void SetAudioOutputClient(ulong guildId, AudioOutStream client);
        public bool TryGetAudioOutputClient(ulong guildId, out AudioOutStream? client);

        public void ClearGuildSession(ulong guildId);

        public IReadOnlyCollection<UserVoiceSession> AllSessions { get; } // To avoid modifying AllSession from anywehre
    }

    //Holds every userVoiceSession and manage them
    internal sealed class VoiceSessionManager : IVoiceSessionManager
    {
        private readonly ConcurrentDictionary<ulong, UserVoiceSession> _byUserId = new();
        private readonly ConcurrentDictionary<ulong, IAudioClient> _audioInputClientByGuildId = new();
        private readonly ConcurrentDictionary<ulong, AudioOutStream> _audioOutputClientByGuildId = new();

        public IReadOnlyCollection<UserVoiceSession> AllSessions => _byUserId.Values.ToArray();

        public UserVoiceSession GetOrCreate(ulong userId, ulong guildId)
        {
            //We look is there existing session, if yes then get it, if no then create it
            var session = _byUserId.GetOrAdd(userId, _ => new UserVoiceSession(userId, guildId));
            session.GuildId = guildId;

            return session;
        }

        public void Remove(ulong userId)
        {
            if (_byUserId.TryRemove(userId, out var session))
                session.Dispose();
        }

        public void ClearGuildSession(ulong guildId)
        {
            if (_audioOutputClientByGuildId.TryRemove(guildId, out var outStream))
            {
                try { outStream.Dispose(); } catch {}
            }
            _audioInputClientByGuildId.TryRemove(guildId, out _);

            var usersInGuild = _byUserId
                .Where(kvp => kvp.Value.GuildId == guildId)
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var userId in usersInGuild)
            {
                Remove(userId);
            }
        }

        public void SetAudioInputClient(ulong guildId, IAudioClient client)
        {
            _audioInputClientByGuildId[guildId] = client;
        }

        public bool TryGetAudioInputClient(ulong guildId, out IAudioClient? client)
        {
            return _audioInputClientByGuildId.TryGetValue(guildId, out client);
        }

        public void SetAudioOutputClient(ulong guildId, AudioOutStream client)
        {
            _audioOutputClientByGuildId[guildId] = client;
        }
        public bool TryGetAudioOutputClient(ulong guildId, out AudioOutStream? client)
        {
            return _audioOutputClientByGuildId.TryGetValue(guildId, out client);
        }

        public bool TryGetByUserId(ulong userId, out UserVoiceSession? session)
        {
            return _byUserId.TryGetValue(userId, out session);
        }
    }
}
