using Telegram.Bot.Types;

namespace CommunityBot.Helpers
{
    public static class UpdateExtensions
    {
        public static string ToLog(this Update update)
        {
            return $"[ID: {update.Id} " +
                   $"| Type: {update.Type} " +
                   $"| MessageId: {ValueOrNullString(update.Message?.MessageId)} " +
                   $"| MessageType: {ValueOrNullString(update.Message?.Type)} " +
                   $"| MessageChatId: {ValueOrNullString(update.Message?.Chat?.Id)}]";
        }

        private static string ValueOrNullString<T>(T value)
        {
            return value?.ToString() ?? "null";
        }
    }
}