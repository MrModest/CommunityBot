using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommunityBot.Contracts;
using CommunityBot.Helpers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using IUpdateHandler = CommunityBot.Contracts.IUpdateHandler;

namespace CommunityBot.Services
{
    public class BotService
    {
        private readonly ILogger<BotService> _logger;
        private readonly IOptions<BotConfigurationOptions> _options;
        private readonly ITelegramBotClient _botClient;
        private readonly IEnumerable<IUpdateHandler> _updateHandlers;

        private bool _isStopPolling;

        public BotService(
            ILogger<BotService> logger,
            IOptions<BotConfigurationOptions> options,
            ITelegramBotClient botClient,
            IEnumerable<IUpdateHandler> updateHandlers)
        {
            _logger = logger;
            _options = options;
            _botClient = botClient;
            _updateHandlers = updateHandlers;
        }

        public async Task StartPolling(TimeSpan? timeout = null)
        {
            _isStopPolling = false;
            
            if (timeout.HasValue)
            {
                _botClient.Timeout = timeout.Value;
            }
            var updateReceiver = new QueuedUpdateReceiver(_botClient);
            updateReceiver.StartReceiving();
            
            _logger.LogInformation("Polling started!");
            
            await foreach (var update in updateReceiver.YieldUpdatesAsync())
            {
                await HandleUpdate(update);

                if (_isStopPolling)
                {
                    _logger.LogInformation("Polling stopped!");
                    break;
                }
            }
        }

        public void StopPolling()
        {
            _isStopPolling = true;
        }

        public Task SetWebhook()
        {
            return _botClient.SetWebhookAsync(_options.Value.WebhookUrl);
        }

        public Task DeleteWebhook()
        {
            return _botClient.DeleteWebhookAsync();
        }

        public Task<WebhookInfo> GetWebhookInfo()
        {
            return _botClient.GetWebhookInfoAsync();
        }

        public async Task HandleUpdate(Update update)
        {
            _logger.LogInformation("Received update [{update}]", update.ToLog());
            
            foreach (var updateHandler in _updateHandlers.OrderByDescending(uh => uh.OrderNumber))
            {
                try
                {
                    await updateHandler.HandleUpdateAsync(update);
                }
                catch (Exception ex)
                {
                    await updateHandler.HandleErrorAsync(ex, update);
                }
            }
            
            _logger.LogInformation("Handled update [{update}]", update.ToLog());
        }
    }
}