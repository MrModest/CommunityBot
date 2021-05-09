using System;
using CommunityBot.Contracts;
using Telegram.Bot.Types;

namespace CommunityBot.Helpers
{
    public static class UserExtensions
    {
        public static string GetMentionHtmlLink(this User user)
        {
            return $"tg://user?id={user.Id}".ToHtmlLink(user.FirstName ?? user.Username);
        }

        public static AppUser ToAppUser(this User user)
        {
            return new()
            {
                Id = user.Id,
                Username = user.Username,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Joined = DateTime.Now,
                AccessType = UserAccessType.Unknown
            };
        }
    }
}
