using Telegram.Bot.Types.ReplyMarkups;

namespace CommunityBot.Helpers
{
    public static class InlineKeyboardHelper
    {
        public static InlineKeyboardMarkup GetPostButtons()
        {
            return new InlineKeyboardMarkup(new []
            {
                InlineKeyboardButton.WithCallbackData("❌ Спам!", "report"), 
                InlineKeyboardButton.WithCallbackData("🧡", "like")
            });
        }
    }
}