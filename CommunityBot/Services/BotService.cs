using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using IUpdateHandler = CommunityBot.Contracts.IUpdateHandler;

namespace CommunityBot.Services
{
    public class BotService
    {
        private readonly ITelegramBotClient _botClient;
        private readonly IEnumerable<IUpdateHandler> _updateHandlers;

        private bool _isStopPolling;

        public BotService(
            ITelegramBotClient botClient,
            IEnumerable<IUpdateHandler> updateHandlers)
        {
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

            await foreach (var update in updateReceiver.YieldUpdatesAsync())
            {
                await HandleUpdate(update);
                
                if (_isStopPolling) { break; }
            }
        }

        public void StopPolling()
        {
            _isStopPolling = true;
        }

        public async Task HandleUpdate(Update update)
        {
            foreach (var updateHandler in _updateHandlers.OrderByDescending(uh => uh.OrderNumber))
            {
                await updateHandler.HandleUpdateAsync(update);
            }
        }
    }
}