using System.Threading.Tasks;
using CommunityBot.Contracts;
using CommunityBot.Handlers.Results;
using CommunityBot.Helpers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace CommunityBot.Handlers.BotCommands
{
    public class GetVersionCommand  : BotCommandHandlerBase
    {
        public GetVersionCommand(
            IOptions<BotConfigurationOptions> options, 
            ILoggerFactory logger)
            : base(options, logger)
        {
        }

        protected override BotCommandConfig Config { get; } = new("version", allowOnlyInPrivate: true);
        protected override Task<IUpdateHandlerResult> HandleUpdateInternal(Update update, string commandArg)
        {
            return Result.Text(update.Message.Chat.Id, Options.Version, update.Message.MessageId).AsTask();
        }
    }
}
