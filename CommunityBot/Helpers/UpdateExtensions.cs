using Telegram.Bot.Types;

namespace CommunityBot.Helpers
{
    public static class UpdateExtensions
    {
        public static string ToLog(this Update update)
        {
            return $"[ID: {update.Id} | Type: {update.Type}]";
        }
    }
}