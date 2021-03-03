using System.Collections.Generic;
using System.IO;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace CommunityBot.Handlers.Results
{
    public static class Result
    {
        public static TextUpdateHandlerResult FromText(long chatId, string text, ParseMode parseMode, bool disableWebPagePreview)
        {
            return new (chatId, text, 0, parseMode, disableWebPagePreview);
        }
        
        public static TextUpdateHandlerResult FromText(long chatId, string text, int replyToMessageId, ParseMode parseMode = ParseMode.Default, bool disableWebPagePreview = false)
        {
            return new (chatId, text, replyToMessageId, parseMode, disableWebPagePreview);
        }

        public static PhotoUpdateHandlerResult FromPhoto(long chatId, string fileId, string caption, ParseMode parseMode = ParseMode.Default, int replyToMessageId = 0)
        {
            return new (chatId, fileId, caption, parseMode, replyToMessageId);
        }

        public static VideoUpdateHandlerResult FromVideo(long chatId, string fileId, string caption, ParseMode parseMode = ParseMode.Default, int replyToMessageId = 0)
        {
            return new (chatId, fileId, caption, parseMode, replyToMessageId);
        }

        public static MediaGroupUpdateHandlerResult FromMediaGroup(long chatId, IEnumerable<IAlbumInputMedia> mediaList, int replyToMessageId = 0)
        {
            return new (chatId, mediaList, replyToMessageId);
        }

        public static DocumentUpdateHandlerResult FromDocument(long chatId, Stream content, string fileName, string caption, ParseMode parseMode = ParseMode.Default, int replyToMessageId = 0)
        {
            return new (chatId, content, fileName, caption, parseMode, replyToMessageId);
        }

        public static AggregateUpdateHandlerResult FromInner(IEnumerable<IUpdateHandlerResult> innerResults)
        {
            return new (innerResults);
        }

        public static NothingUpdateHandlerResult Nothing()
        {
            return new ();
        }
    }
}