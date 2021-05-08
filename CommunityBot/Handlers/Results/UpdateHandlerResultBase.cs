using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace CommunityBot.Handlers.Results
{
    public abstract class UpdateHandlerResultBase : IUpdateHandlerResult
    {
        protected UpdateHandlerResultBase(long chatId, string text, ParseMode parseMode, int replyToMessageId)
        {
            ChatId = new ChatId(chatId);
            Text = text;
            ParseMode = parseMode;
            ReplyToMessageId = replyToMessageId;
        }

        public ChatId ChatId { get; }

        protected string Text { get; }

        public ParseMode ParseMode { get; }

        public int ReplyToMessageId { get; }
    }
}