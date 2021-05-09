using System;
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

namespace CommunityBot.Handlers
{
    public abstract class UpdateHandlerBase : IUpdateHandler
    {
        protected readonly BotConfigurationOptions Options;
        protected readonly ILogger Logger;

        public string HandlerName => GetType().Name;

        protected UpdateHandlerBase(
            IOptions<BotConfigurationOptions> options,
            ILogger logger)
        {
            Options = options.Value;
            Logger = logger;
        }
        
        protected UpdateHandlerBase(
            IOptions<BotConfigurationOptions> options,
            ILoggerFactory loggerFactory)
        {
            Options = options.Value;
            Logger = loggerFactory.CreateLogger(GetType());
        }
        
        protected abstract UpdateType[] AllowedUpdates { get; }

        public virtual int OrderNumber => 0;

        public async Task<IUpdateHandlerResult> HandleUpdateAsync(Update update)
        {
            if (!AllowedUpdates.Contains(update.Type) || !CanHandle(update))
            {
                Logger.LogTrace("Can't handle with '{HandlerName}' | {Update}", HandlerName, update.ToLog());
                return Result.Nothing();
            }
            
            Logger.LogTrace("Start handler: '{HandlerName}' | {Update}", HandlerName, update.ToLog());

            IUpdateHandlerResult result;

            try
            {
                result = await HandleUpdateInternal(update);
            }
            catch (Exception ex)
            {
                Logger.LogError("Caught exception in handler: '{HandlerName}' | [{Update}] | {ExMessage} | \n\n{ExStackTrace}", 
                    HandlerName, update?.ToLog(), ex.Message, ex.StackTrace);
                
                result = await HandleErrorInternalAsync(ex, update);
            }

            Logger.LogTrace("End handler: '{HandlerName}'", HandlerName);

            return result;
        }

        protected abstract bool CanHandle(Update update);
        
        protected abstract Task<IUpdateHandlerResult> HandleUpdateInternal(Update update);

        protected virtual Task<IUpdateHandlerResult> HandleErrorInternalAsync(Exception exception, Update? update = null)
        {
            Logger.LogWarning("Default handler was called in '{HandlerName}'! [{Update}]", HandlerName, update?.ToLog());

            return Result.Error(Options.DebugInfoChatIds, HandlerName, exception).AsTask();
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
        
        protected static IUpdateHandlerResult ReplyPlainText(Update update, string text)
        {
            return Result.Text(update.Message.Chat.Id, text, update.Message.MessageId);
        }
    }
}
