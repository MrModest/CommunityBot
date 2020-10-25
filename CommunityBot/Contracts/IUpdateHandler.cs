using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace CommunityBot.Contracts
{
    public interface IUpdateHandler
    {
        int OrderNumber { get; }
        
        Task HandleUpdateAsync(Update update);
        
        Task HandleErrorAsync(Exception exception, Update? update = null);
    }
}