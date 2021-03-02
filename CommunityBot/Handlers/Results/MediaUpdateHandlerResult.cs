using Telegram.Bot.Types.Enums;

namespace CommunityBot.Handlers.Results
{
    public abstract class MediaUpdateHandlerResult : UpdateHandlerResultBase
    {
        public MediaUpdateHandlerResult(long chatId, string text, string fileId, ParseMode parseMode = ParseMode.Default, int replyToMessageId = 0)
            : base(chatId, text, parseMode, replyToMessageId)
        {
            FileId = fileId;
        }

        public string Caption => Text;

        public string FileId { get; }
    }

    public class PhotoUpdateHandlerResult : MediaUpdateHandlerResult
    {
        public PhotoUpdateHandlerResult(long chatId, string text, string fileId, ParseMode parseMode = ParseMode.Default, int replyToMessageId = 0)
            : base(chatId, text, fileId, parseMode, replyToMessageId)
        {
        }
    }
    
    public class VideoUpdateHandlerResult : MediaUpdateHandlerResult
    {
        public VideoUpdateHandlerResult(long chatId, string text, string fileId, ParseMode parseMode = ParseMode.Default, int replyToMessageId = 0)
            : base(chatId, text, fileId, parseMode, replyToMessageId)
        {
        }
    }
}