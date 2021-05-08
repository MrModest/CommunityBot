using System;
using System.Threading;
using System.Threading.Tasks;
using CommunityBot.Handlers.Results;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace CommunityBot.Contracts
{
    public interface IUpdateHandler
    {
        int OrderNumber { get; }
        
        string HandlerName { get; }
        
        Task<IUpdateHandlerResult> HandleUpdateAsync(Update update);
    }
}
