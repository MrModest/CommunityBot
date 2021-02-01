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
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var isAdminString = context.HttpContext.Request.Headers["CurrentUserIsAdmin"].FirstOrDefault();
            

            if (isAdminString.IsBlank() || bool.TryParse(isAdminString, out var isAdmin) || !isAdmin)
            {
                context.Result = new ForbidResult();
                return;
            }

            await next();
        }
    }
}