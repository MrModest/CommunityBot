using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace CommunityBot.Handlers.Results
{
    public class TextUpdateHandlerResult : UpdateHandlerResultBase
    {
        public TextUpdateHandlerResult(long chatId, string text, int replyToMessageId, ParseMode parseMode = ParseMode.Default, bool disableWebPagePreview = false, IReplyMarkup? replyMarkup = null)
            : base(chatId, text, parseMode, replyToMessageId, replyMarkup)
        {
            DisableWebPagePreview = disableWebPagePreview;
        }

        public string MessageText => Text;

        public bool DisableWebPagePreview { get; }
    }
}
