using System.Threading.Tasks;
using CommunityBot.Contracts;
using CommunityBot.Handlers.Results;
using CommunityBot.Helpers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace CommunityBot.Handlers.BotCommands.ChatManage
{
    public class GetIdOfThisChatBotCommand : BotCommandHandlerBase
    {
        public GetIdOfThisChatBotCommand(
            ITelegramBotClient botClient, 
            IOptions<BotConfigurationOptions> options, 
            ILoggerFactory logger)
            : base(botClient, options, logger)
        {
        }

        protected override BotCommandConfig Config { get; } = new ("get_id_of_this_chat");
        protected override Task<IUpdateHandlerResult> HandleUpdateInternal(Update update, string commandArg)
        {
            return Result.Text(update.Message.Chat.Id, $"ID этого чата: {update.Message.Chat.Id}", update.Message.MessageId).AsTask();
        }
    }
}