using System;
using System.Linq;
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
            ILogger logger)
        {
            BotClient = botClient;
            Options = options.Value;
            Logger = logger;
        }
        
        protected abstract UpdateType[] AllowedUpdates { get; }

        public virtual int OrderNumber => 0;

        public async Task HandleUpdateAsync(Update update)
        {
            if (!AllowedUpdates.Contains(update.Type) || !CanHandle(update))
            {
                Logger.LogTrace("Can't handle with '{handlerName}' | {update}", HandlerName, update.ToLog());
                return;
            }
            
            Logger.LogInformation("Start handler: '{handlerName}' | {update}", HandlerName, update.ToLog());

            await HandleUpdateInternalAsync(update);
            
            Logger.LogInformation("End handler: '{handlerName}'", HandlerName);
        }

        public async Task HandleErrorAsync(Exception exception, Update? update = null)
        {
            Logger.LogError("Caught exception in handler: '{handlerName}' | [{update}] | {exMessage} | {exStackTrace}", 
                HandlerName, update?.ToLog(), exception.Message, exception.StackTrace);

            await HandleErrorInternalAsync(exception, update);
        }

        protected abstract bool CanHandle(Update update);
        
        protected abstract Task HandleUpdateInternalAsync(Update update);

        protected virtual async Task HandleErrorInternalAsync(Exception exception, Update? update = null)
        {
            Logger.LogWarning("Default handler was called in '{handlerName}'! [{update}]", HandlerName, update?.ToLog());

            foreach (var debugInfoChatId in Options.DebugInfoChatIds)
            {
                await BotClient.SendTextMessageAsync(debugInfoChatId,
                    $"Exception was throwed in handler '{HandlerName}':\n\n{exception.Message}\n\n{exception.StackTrace}");
            }
        }

        protected bool IsFromAdmin(Update update)
        {
            var fromUser = update.Type switch
            {
                UpdateType.Message => update.Message.From,
                UpdateType.ChannelPost => update.ChannelPost.From,
                UpdateType.InlineQuery => update.InlineQuery.From,
                UpdateType.ChosenInlineResult => update.ChosenInlineResult.From,
                UpdateType.CallbackQuery => update.CallbackQuery.From,
                UpdateType.EditedMessage => update.EditedMessage.From,
                UpdateType.EditedChannelPost => update.EditedChannelPost.From,
                _ => null
            };

            return Options.Admins.Contains(fromUser?.Username);
        }
    }
}