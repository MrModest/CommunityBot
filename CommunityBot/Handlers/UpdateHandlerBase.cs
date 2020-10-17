using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommunityBot.Contracts;
using CommunityBot.Helpers;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace CommunityBot.Handlers
{
    public abstract class UpdateHandlerBase : IUpdateHandler
    {
        protected readonly ITelegramBotClient BotClient;
        protected readonly ILogger Logger;

        private string HandlerName => GetType().Name;

        protected UpdateHandlerBase(
            ITelegramBotClient botClient,
            ILogger<UpdateHandlerBase> logger)
        {
            BotClient = botClient;
            Logger = logger;
        }
        
        protected virtual UpdateType[] AllowedUpdates => Array.Empty<UpdateType>();

        public virtual int OrderNumber => 0;

        public async Task HandleUpdateAsync(Update update)
        {
            Logger.LogInformation($"Start handler: '{HandlerName}' | {update.ToLog()}");

            if (AllowedUpdates.Contains(update.Type))
            {
               await HandleUpdateInternalAsync(update);
            }
            
            Logger.LogInformation($"End handler: '{HandlerName}'");
        }
        
        protected abstract Task HandleUpdateInternalAsync(Update update);
    }
}