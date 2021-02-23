using System;
using System.Linq;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace CommunityBot.Helpers
{
    public static class MessageExtensions
    {
        public static bool IsBotCommand(this Message message)
        {
            return message.GetEntities().Any(e => e.Type == MessageEntityType.BotCommand);
        }

        public static bool IsMention(this Message message)
        {
            return message.GetEntities().Any(e => e.Type == MessageEntityType.Mention);
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

        public static MessageEntity[] GetEntities(this Message message)
        {
            return message.Type == MessageType.Text 
                ? message.Entities.EmptyIfNull() 
                : message.CaptionEntities.EmptyIfNull();
        }
        
        public static (string name, string arg)? GetFirstBotCommand(this Message message)
        {
            var entity = message.GetEntities()
                .FirstOrDefault(e => e.Type == MessageEntityType.BotCommand);

            if (entity == null)
            {
                return null;
            }

            return (
                name: message.Text.Substring(entity.Offset + 1, entity.Length - 1),
                arg: message.Text.Remove(entity.Offset, entity.Length).Trim()
            );
        }

        private static string[] GetMentionedUserNames(this Message message)
        {
            return message.GetEntities()
                .Where(e => e.Type == MessageEntityType.Mention)
                .Select(e => message.Text.Substring(e.Offset + 1, e.Length - 1))
                .ToArray();
        }
        
        public static bool HasMentionOfUserName(this Message message, string username)
        {
            return message.GetMentionedUserNames()
                .Any(m => m.Equals(username, StringComparison.OrdinalIgnoreCase));
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
    }
}
