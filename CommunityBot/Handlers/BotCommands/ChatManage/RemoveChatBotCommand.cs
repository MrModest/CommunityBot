using System.Threading.Tasks;
using CommunityBot.Contracts;
using CommunityBot.Handlers.Results;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace CommunityBot.Handlers.BotCommands.ChatManage
{
    public class RemoveChatBotCommand : BotCommandHandlerBase
    {
        private readonly IChatRepository _chatRepository;

        public RemoveChatBotCommand(
            IOptions<BotConfigurationOptions> options,
            IChatRepository chatRepository,
            ILoggerFactory logger)
            : base(options, logger)
        {
            _chatRepository = chatRepository;
        }

        protected override BotCommandConfig Config { get; } =
            new ("remove_chat", "Напиши рядом с командой полное имя чата, который удаляем.");

        protected override async Task<IUpdateHandlerResult> HandleUpdateInternal(Update update, string chatExactName)
        {
            await _chatRepository.RemoveByName(chatExactName);

            return Result.Text(
                update.Message.Chat.Id, 
                $"Если чат с названием {chatExactName} существовал в моём списке, то я его удалил.", 
                update.Message.MessageId);
        }
    }
}
