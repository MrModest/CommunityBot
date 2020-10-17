using Telegram.Bot.Types;

namespace CommunityBot.Helpers
{
    public static class UserExtensions
    {
        public static string GetMentionMdLink(this User user)
        {
            return $"tg://user?id={user.Id}".ToMdLink(user.FirstName ?? user.Username);
        }
    }
}