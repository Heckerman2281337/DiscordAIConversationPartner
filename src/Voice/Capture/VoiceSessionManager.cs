using System.Collections.Concurrent;

namespace DiscordVoiceBotMark.src.Voice
{
    //For DI
    internal interface IVoiceSessionManager
    {
        public UserVoiceSession GetOrCreate(ulong userId, ulong channelId);
        public bool TryGetByUserId(ulong userId, out UserVoiceSession? session);
        public void Remove(ulong userId);
        public IReadOnlyCollection<UserVoiceSession> AllSessions { get; } // To avoid modifying AllSession from anywehre
    }

    //Holds every userVoiceSession and manage them
    internal sealed class VoiceSessionManager : IVoiceSessionManager
    {
        private readonly ConcurrentDictionary<ulong, UserVoiceSession> _byUserId = new();
        
        public IReadOnlyCollection<UserVoiceSession> AllSessions => _byUserId.Values.ToArray();

        public UserVoiceSession GetOrCreate(ulong userId, ulong channelId)
        {
            //We look is there existing session, if yes then get it, if no then create it
            var session = _byUserId.GetOrAdd(userId, channelId => new UserVoiceSession(userId, channelId));
            session.ChannelId = channelId;

            return session;
        }

        public void Remove(ulong userId)
        {
            if (_byUserId.TryRemove(userId, out var session))
                session.Dispose();
        }


        public bool TryGetByUserId(ulong userId, out UserVoiceSession? session)
        {
            return _byUserId.TryGetValue(userId, out session);
        }
    }
}
