using System.Linq;
using System.Threading.Tasks;
using CommunityBot.Contracts;
using CommunityBot.Handlers.Results;
using CommunityBot.Helpers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace CommunityBot.Handlers.BotCommands.ChatManage
{
    public class GetAllChatsBotCommand : BotCommandHandlerBase
    {
        private readonly IChatRepository _chatRepository;

        public GetAllChatsBotCommand(
            ITelegramBotClient botClient, 
            IOptions<BotConfigurationOptions> options,
            IChatRepository chatRepository,
            ILoggerFactory logger)
            : base(botClient, options, logger)
        {
            _chatRepository = chatRepository;
        }

        protected override BotCommandConfig Config { get; } = new ("get_all_chats", true);
        protected override async Task<IUpdateHandlerResult> HandleUpdateInternal(Update update, string commandArg)
        {
            if (!update.Message.IsPrivate())
            {
                return Result.Text(update.Message.Chat.Id, "Смотреть список всех чатов можно только в ЛС.", update.Message.MessageId);
            }

            var chats = await _chatRepository.GetAll();
            
            var chatMarkup = chats.Select(c =>
                    $"{c.JoinLink.ToHtmlLink(c.ExactName)}\n{c.JoinLink.ToMonospace()}")
                .ToArray();

            var resultMessage = chatMarkup.Any()
                ? string.Join("\n\n", chatMarkup) 
                : "Список чатов пуст!\nЕго можно пополнить при помощи команды /add_chat";

            return Result.Text(update.Message.Chat.Id, resultMessage, update.Message.MessageId, ParseMode.Html);
        }
    }
}