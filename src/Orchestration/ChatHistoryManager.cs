
using System.Collections.Concurrent;

namespace DiscordVoiceBotMark.Orchestration
{
    public record ChatMessage(string Role, string Content);

    public interface IChatHistoryManager
    {
        public IReadOnlyList<ChatMessage> GetHistory(ulong guildId);
        public void AddMessage(ulong guildId, string role, string content);
        public void ClearHistory(ulong guildId);
    }

    internal sealed class ChatHistoryManager : IChatHistoryManager
    {
        private readonly ConcurrentDictionary<ulong, List<ChatMessage>> _histories = new();

        public IReadOnlyList<ChatMessage> GetHistory(ulong guildId)
        {
            // if no history for channel - add new history
            var history = GetOrCreateHistory(guildId);

            lock (history)
            {
                return history.ToList();
            }
        }

        public void AddMessage(ulong guildId, string role, string content)
        {
            var history = GetOrCreateHistory(guildId);
            var newMessage = new ChatMessage(role, content);

            lock (history) // protect from collision(if more than 1 user speaks at the same time)
            {
                history.Add(newMessage);

                if (history.Count > 20)
                {
                    history.RemoveAt(0);
                }
            }
        }

        public void ClearHistory(ulong guildId)
        {
            _histories.TryRemove(guildId, out _);
        }

        private List<ChatMessage> GetOrCreateHistory(ulong guildId)
        {
            return _histories.GetOrAdd(guildId, _ => new List<ChatMessage>());
        }
    }
}
