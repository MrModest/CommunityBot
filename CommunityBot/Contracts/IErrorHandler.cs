using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;

namespace CommunityBot.Contracts
{
    public interface IErrorHandler
    {
        Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken);
    }
}