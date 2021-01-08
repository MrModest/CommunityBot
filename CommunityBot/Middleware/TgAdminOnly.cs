using System.Linq;
using System.Threading.Tasks;
using CommunityBot.Contracts;
using CommunityBot.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;

namespace CommunityBot.Middleware
{
    public class TgAdminOnly : IAsyncActionFilter
    {
        private readonly BotConfigurationOptions _botConfigurationOptions;

        public TgAdminOnly(
            IOptions<BotConfigurationOptions> botConfigurationOptions)
        {
            _botConfigurationOptions = botConfigurationOptions.Value;
        }
        
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            //var currentUserTgId = context.HttpContext.Request.Headers["CurrentUserTgId"].FirstOrDefault();
            var currentUserTgUserName = context.HttpContext.Request.Headers["CurrentUserTgUserName"].FirstOrDefault();

            if (currentUserTgUserName.IsBlank() || !_botConfigurationOptions.Admins.Contains(currentUserTgUserName))
            {
                context.Result = new ForbidResult();
                return;
            }

            await next();
        }
    }
}