using System.Linq;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace CommunityBot.Helpers
{
    public static class MessageExtensions
    {
        public static bool IsBotCommand(this Message message)
        {
            return message.Entities?.Any(e => e.Type == MessageEntityType.BotCommand) ?? false;
        }

        public static bool IsMention(this Message message)
        {
            return message.Entities?.Any(e => e.Type == MessageEntityType.Mention) ?? false;
        }

        public static bool IsPrivate(this Message message)
        {
            return message.Chat.Type == ChatType.Private;
        }

        public static string GetPostLink(this Message message)
        {
            return $"https://t.me/c/{message.Chat.Id.ToString().Substring(4)}/{message.MessageId}";
        }
        
        public static (string name, string arg)[] GetBotCommands(this Message message)
        {
            return message.Entities?
                .Where(e => e.Type == MessageEntityType.BotCommand)
                .Select(e => (
                    name: message.Text.Substring(e.Offset + 1, e.Length - 1), 
                    arg: message.Text.Remove(e.Offset, e.Length)))
                .ToArray()
                    ?? new (string name, string arg)[0];
        }

        public static string[] GetMentionedUserNames(this Message message)
        {
            return message.Entities?
                .Where(e => e.Type == MessageEntityType.Mention)
                .Select(e => message.Text.Substring(e.Offset + 1, e.Length - 1))
                .ToArray()
                   ?? new string[0];
        }
    }
}