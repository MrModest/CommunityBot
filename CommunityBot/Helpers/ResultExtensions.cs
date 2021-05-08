using System.Threading.Tasks;
using CommunityBot.Handlers.Results;

namespace CommunityBot.Helpers
{
    public static class ResultExtensions
    {
        public static Task<IUpdateHandlerResult> AsTask(this IUpdateHandlerResult result)
        {
            return Task.FromResult(result);
        }
    }
}