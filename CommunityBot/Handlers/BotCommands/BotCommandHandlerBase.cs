using System.Threading.Tasks;
using CommunityBot.Contracts;
using CommunityBot.Handlers.Results;
using CommunityBot.Helpers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace CommunityBot.Handlers.BotCommands
{
    public abstract class BotCommandHandlerBase : UpdateHandlerBase
    {
        public BotCommandHandlerBase( 
            IOptions<BotConfigurationOptions> options, 
            ILoggerFactory logger)
            : base(options, logger)
        {
        }

        protected override UpdateType[] AllowedUpdates => new[] {UpdateType.Message};

        protected abstract BotCommandConfig Config { get; }
        
        protected override bool CanHandle(Update update)
        {
            return update.Message.ContainCommand(Config.BotCommand);
        }

        protected override async Task<IUpdateHandlerResult> HandleUpdateInternal(Update update)
        {
            if (Config.IsForAdmin && !IsFromAdmin(update))
            {
                return Result.Text(update.Message.Chat.Id, "Данная команда доступна только администраторам!", update.Message.MessageId);
            }

            if (Config.AllowOnlyInPrivate && !update.Message.IsPrivate())
            {
                return Result.Text(update.Message.Chat.Id, "Команда доступна только в ЛС", update.Message.MessageId);
            }
            
            var (_, commandArg) = update.Message.GetFirstBotCommand()!.Value;
            
            if (Config.ArgRequiredMessage.IsNotBlank() && commandArg.IsBlank())
            {
                return Result.Text(update.Message.Chat.Id, Config.ArgRequiredMessage!, update.Message.MessageId);
            }

            return await HandleUpdateInternal(update, commandArg);
        }

        protected abstract Task<IUpdateHandlerResult> HandleUpdateInternal(Update update, string commandArg);
    }
}
