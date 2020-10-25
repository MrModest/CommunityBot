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

        public static InputMediaPhoto ToInputMedia(this IEnumerable<PhotoSize> photoSizes)
        {
            var fileId = photoSizes.GetLargestPhotoSize().FileId;
            
            return new InputMediaPhoto(fileId);
        }
        
        public static InputMediaVideo ToInputMedia(this Video video)
        {
            return new InputMediaVideo(video.FileId);
        }
    }
}