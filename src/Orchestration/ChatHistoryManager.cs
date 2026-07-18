
using System.Collections.Concurrent;

namespace DiscordVoiceBotMark.Orchestration
{
    public interface IChatHistoryManager
    {
        public List<object> GetHistory(ulong guildId);
        public void AddMessage(ulong guildId, string role, string content);
        public void ClearHistory(ulong guildId);
    }

    internal sealed class ChatHistoryManager : IChatHistoryManager
    {
        private readonly ConcurrentDictionary<ulong, List<object>> _histories = new();

        public List<object> GetHistory(ulong guildId)
        {
            // if no history for channel - add new history
            return _histories.GetOrAdd(guildId, _ => new List<object>());
        }

        public void AddMessage(ulong guildId, string role, string content)
        {
            var history = GetHistory(guildId);

            lock (history) // protect from collision(if more than 1 user speaks at the same time)
            {
                history.Add(new {role, content});

                if(history.Count > 20)
                {
                    history.RemoveAt(0);
                }
            }
        }

        public void ClearHistory(ulong guildId)
        {
            _histories.TryRemove(guildId, out _);
        }
    }
}
