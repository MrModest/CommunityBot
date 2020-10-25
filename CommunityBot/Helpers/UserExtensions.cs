using Telegram.Bot.Types;

namespace CommunityBot.Helpers
{
    public static class UserExtensions
    {
        public static string GetMentionMdLink(this User user)
        {
            return $"tg://user?id={user.Id}".ToMdLink(user.FirstName ?? user.Username);
        }

        public static string GetMentionHtmlLink(this User user)
        {
            return $"tg://user?id={user.Id}".ToHtmlLink(user.FirstName ?? user.Username);
        }
    }
}