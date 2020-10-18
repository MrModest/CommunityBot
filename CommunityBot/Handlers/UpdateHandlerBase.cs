using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommunityBot.Contracts;
using CommunityBot.Helpers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace CommunityBot.Handlers
{
    public abstract class UpdateHandlerBase : IUpdateHandler
    {
        protected readonly ITelegramBotClient BotClient;
        protected readonly BotConfigurationOptions Options;
        protected readonly ILogger Logger;

        private string HandlerName => GetType().Name;

        protected UpdateHandlerBase(
            ITelegramBotClient botClient,
            IOptions<BotConfigurationOptions> options,
            ILogger<UpdateHandlerBase> logger)
        {
            BotClient = botClient;
            Options = options.Value;
            Logger = logger;
        }
        
        protected virtual UpdateType[] AllowedUpdates => Array.Empty<UpdateType>();

        public virtual int OrderNumber => 0;

        public async Task HandleUpdateAsync(Update update)
        {
            Logger.LogInformation($"Start handler: '{HandlerName}' | {update.ToLog()}");

            if (AllowedUpdates.Contains(update.Type) && CanHandle(update))
            {
               await HandleUpdateInternalAsync(update);
            }
            
            Logger.LogInformation($"End handler: '{HandlerName}'");
        }

        protected abstract bool CanHandle(Update update);
        
        protected abstract Task HandleUpdateInternalAsync(Update update);
    }
}