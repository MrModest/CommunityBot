using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace CommunityBot.Handlers
{
    public static class ChatExtensions
    {
        public static bool IsPrivate(this Chat chat)
        {
            return chat.Type == ChatType.Private;
        }

        public static bool IsGroup(this Chat chat)
        {
            return chat.Type == ChatType.Supergroup ||
                   chat.Type == ChatType.Group;
        }
    }
}