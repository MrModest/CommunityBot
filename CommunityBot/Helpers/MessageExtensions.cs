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

        public static bool IsGroup(this Message message)
        {
            return message.Chat.Type == ChatType.Supergroup ||
                   message.Chat.Type == ChatType.Group;
        }

        public static string GetPostLink(this Message message)
        {
            return $"https://t.me/c/{message.Chat.Id.ToString().Substring(4)}/{message.MessageId}";
        }
        
        public static (string name, string arg)? GetFirstBotCommand(this Message message)
        {
            var entity = message.Entities?
                .FirstOrDefault(e => e.Type == MessageEntityType.BotCommand);

            return entity != null
                ? (
                    name: message.Text.Substring(entity.Offset + 1, entity.Length - 1),
                    arg: message.Text.Remove(entity.Offset, entity.Length).Trim()
                  )
                : ((string name, string arg)?)null;
        }

        public static string? GetArgIfIsCommandOf(this Message message, string commandName)
        {
            var command = message.GetFirstBotCommand();

            return command?.name == commandName
                ? command.Value.arg
                : null;
        }

        public static string[] GetMentionedUserNames(this Message message)
        {
            return message.Entities?
                .Where(e => e.Type == MessageEntityType.Mention)
                .Select(e => message.Text.Substring(e.Offset + 1, e.Length - 1))
                .ToArray()
                   ?? new string[0];
        }

        public static IAlbumInputMedia? ToInputMedia(this Message message)
        {
            switch (message.Type)
            {
                case MessageType.Photo:
                    var photo = new InputMediaPhoto(message.Photo.GetLargestPhotoSize().FileId);
                    photo.Caption = message.Caption;
                    photo.ParseMode = ParseMode.Html;
                    return photo;
                
                case MessageType.Video:
                    var video = new InputMediaVideo(message.Video.FileId);
                    video.Caption = message.Caption;
                    video.ParseMode = ParseMode.Html;
                    return video;
                
                default:
                    return null;
            }
        }
        
        public static InputMediaVideo ToInputMedia(this Video video)
        {
            return new InputMediaVideo(video.FileId);
        }
    }
}