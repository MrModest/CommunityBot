using Telegram.Bot.Types.Enums;

namespace CommunityBot.Handlers.Results
{
    public class TextUpdateHandlerResult : UpdateHandlerResultBase
    {
        public TextUpdateHandlerResult(long chatId, string text, int replyToMessageId, ParseMode parseMode = ParseMode.Default, bool disableWebPagePreview = false)
            : base(chatId, text, parseMode, replyToMessageId)
        {
            DisableWebPagePreview = disableWebPagePreview;
        }

        public string MessageText => Text;

        public bool DisableWebPagePreview { get; }
    }
}