using System.Collections.Generic;
using System.Linq;
using Telegram.Bot.Types;

namespace CommunityBot.Helpers
{
    public static class InputMediaExtensions
    {
        public static PhotoSize GetLargestPhotoSize(this IEnumerable<PhotoSize> photoSizes)
        {
            return photoSizes.OrderByDescending(ps => ps.FileSize).First();
        }
    }
}