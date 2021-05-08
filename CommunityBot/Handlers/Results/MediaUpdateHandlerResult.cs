using Telegram.Bot.Types.Enums;

namespace CommunityBot.Handlers.Results
{
    public abstract class MediaUpdateHandlerResult : UpdateHandlerResultBase
    {
        public MediaUpdateHandlerResult(long chatId, string fileId, string caption, ParseMode parseMode = ParseMode.Default, int replyToMessageId = 0)
            : base(chatId, caption, parseMode, replyToMessageId)
        {
            FileId = fileId;
        }

        public string Caption => Text;

        public string FileId { get; }
    }

    public class PhotoUpdateHandlerResult : MediaUpdateHandlerResult
    {
        public PhotoUpdateHandlerResult(long chatId, string fileId, string caption, ParseMode parseMode = ParseMode.Default, int replyToMessageId = 0)
            : base(chatId, fileId, caption, parseMode, replyToMessageId)
        {
        }
    }
    
    public class VideoUpdateHandlerResult : MediaUpdateHandlerResult
    {
        public VideoUpdateHandlerResult(long chatId, string fileId, string caption, ParseMode parseMode = ParseMode.Default, int replyToMessageId = 0)
            : base(chatId, fileId, caption, parseMode, replyToMessageId)
        {
        }
    }
}