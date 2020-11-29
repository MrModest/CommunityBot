using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace CommunityBot.Helpers
{
    public static class UpdateExtensions
    {
        public static string ToLog(this Update update)
        {
            var message = update.GetMessage();

            var additionalInfo = message != null
                ? $"| MessageId: {ValueOrNullString(message.MessageId)} " +
                  $"| MessageType: {ValueOrNullString(message.Type)} " +
                  $"| MessageChatId: {ValueOrNullString(message.Chat?.Id)}]" +
                  $"| MessageChatTitle: {ValueOrNullString(message.Chat?.Title ?? $"{message.Chat?.FirstName} {message.Chat?.LastName}")}"
                : string.Empty;

            return $"[ ID: {update.Id} " +
                   $"| Type: {update.Type} " +
                   additionalInfo + " ]";
        }

        private static Message? GetMessage(this Update update)
        {
            return update.Type switch
            {
                UpdateType.Message => update.Message,
                UpdateType.EditedMessage => update.EditedMessage,
                UpdateType.ChannelPost => update.ChannelPost,
                UpdateType.EditedChannelPost => update.EditedChannelPost,
                _ => null
            };
        }

        private static string ValueOrNullString<T>(T value)
        {
            return value?.ToString() ?? "null";
        }
    }
}