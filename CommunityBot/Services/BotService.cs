using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommunityBot.Helpers;
using Serilog;
using Telegram.Bot;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using IUpdateHandler = CommunityBot.Contracts.IUpdateHandler;

namespace CommunityBot.Services
{
    public class BotService
    {
        private readonly ILogger _logger;
        private readonly ITelegramBotClient _botClient;
        private readonly IEnumerable<IUpdateHandler> _updateHandlers;

        private bool _isStopPolling;

        public BotService(
            ILogger logger,
            ITelegramBotClient botClient,
            IEnumerable<IUpdateHandler> updateHandlers)
        {
            _logger = logger;
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
            
            _logger.Information("Polling started!");
            throw new Exception("test exception");

            await foreach (var update in updateReceiver.YieldUpdatesAsync())
            {
                await HandleUpdate(update);

                if (_isStopPolling)
                {
                    _logger.Information("Polling stopped!");
                    break;
                }
            }
        }

        public void StopPolling()
        {
            _isStopPolling = true;
        }

        public async Task HandleUpdate(Update update)
        {
            _logger.Debug("Received update [{update}]", update.ToLog());
            
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
            
            _logger.Debug("Handled update [{update}]", update.ToLog());
        }
    }
}