using System.Threading.Tasks;
using CommunityBot.Contracts;
using CommunityBot.Handlers.Results;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace CommunityBot.Handlers.BotCommands.User
{
    public class ShowAdminsCommand : BotCommandHandlerBase
    {
        public ShowAdminsCommand(
            ITelegramBotClient botClient, 
            IOptions<BotConfigurationOptions> options, 
            ILoggerFactory logger)
            : base(botClient, options, logger)
        {
        }

        protected override BotCommandConfig Config { get; } = new("show_admins", allowOnlyInPrivate: true);
        protected override async Task<IUpdateHandlerResult> HandleUpdateInternal(Update update, string commandArg)
        {
            return Result.Text(update.Message.Chat.Id,
                $"Список логинов админов бота: \n\n{string.Join("\n@", Options.Admins)}\n\n" +
                    $"Твой логин: {update.Message.From.Username}\n\n" + 
                    $"Ты {(IsFromAdmin(update) ? "" : "не ")}админ.",
                update.Message.MessageId);
        }
    }
}
