using Telegram.Bot.Types;

namespace CommunityBot.Helpers
{
    public static class UserExtensions
    {
        public static string GetMentionHtmlLink(this User user)
        {
            return $"tg://user?id={user.Id}".ToHtmlLink(user.FirstName ?? user.Username);
        }
    }
}