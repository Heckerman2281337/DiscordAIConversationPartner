
using System.Collections.Concurrent;

namespace DiscordVoiceBotMark.src.Orchestration
{
    public interface IChatHistoryManager
    {
        public List<object> GetHistory(ulong channelId);
        public void AddMessage(ulong channelId, string role, string content);
        public void ClearHistory(ulong channelId);
    }

    internal sealed class ChatHistoryManager : IChatHistoryManager
    {
        private readonly ConcurrentDictionary<ulong, List<object>> _histories = new();

        public List<object> GetHistory(ulong channelId)
        {
            // if no history for channel - add new history
            return _histories.GetOrAdd(channelId, _ => new List<object>());
        }

        public void AddMessage(ulong channelId, string role, string content)
        {
            var history = GetHistory(channelId);

            lock (history) // protect from collision(if more than 1 user speaks at the same time)
            {
                history.Add(new {role, content});

                if(history.Count > 20)
                {
                    history.RemoveAt(0);
                }
            }
        }

        public void ClearHistory(ulong channelId)
        {
            _histories.TryRemove(channelId, out _);
        }
    }
}
