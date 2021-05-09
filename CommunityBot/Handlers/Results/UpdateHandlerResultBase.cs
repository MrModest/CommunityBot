using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace CommunityBot.Handlers.Results
{
    public abstract class UpdateHandlerResultBase : IUpdateHandlerResult
    {
        protected UpdateHandlerResultBase(long chatId, string text, ParseMode parseMode, int replyToMessageId, IReplyMarkup? replyMarkup)
        {
            ChatId = new ChatId(chatId);
            Text = text;
            ParseMode = parseMode;
            ReplyToMessageId = replyToMessageId;
            ReplyMarkup = replyMarkup;
        }

        public ChatId ChatId { get; }

        protected string Text { get; }

        public ParseMode ParseMode { get; }

        public int ReplyToMessageId { get; }
        
        public IReplyMarkup? ReplyMarkup { get; }
    }
}
