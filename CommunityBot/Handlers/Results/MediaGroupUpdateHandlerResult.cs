using System.Collections.Generic;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace CommunityBot.Handlers.Results
{
    public class MediaGroupUpdateHandlerResult : UpdateHandlerResultBase
    {
        public MediaGroupUpdateHandlerResult(long chatId, IEnumerable<IAlbumInputMedia> mediaList, int replyToMessageId = 0)
            : base(chatId, string.Empty, ParseMode.Default, replyToMessageId, null)
        {
            MediaList = mediaList;
        }
        
        public IEnumerable<IAlbumInputMedia> MediaList { get; }
    }
}
