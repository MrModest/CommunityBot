using System;
using System.Linq;
using System.Threading.Tasks;
using CommunityBot.Contracts;
using CommunityBot.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CommunityBot.Middleware
{
    public class TgAuthorizationFilter : IAsyncActionFilter
    {
        private readonly IAppUserRepository _appUserRepository;

        public TgAuthorizationFilter(
            IAppUserRepository appUserRepository)
        {
            _appUserRepository = appUserRepository;
        }
        
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (next == null)
            {
                throw new ArgumentNullException(nameof(next));
            }
            
            var username = context.HttpContext.Request.Query["username"].FirstOrDefault();
            var idStr = context.HttpContext.Request.Query["tgUserId"].FirstOrDefault();
            var password = context.HttpContext.Request.Query["password"].FirstOrDefault();

            if (password.IsBlank() || (username.IsBlank() && idStr.IsBlank()))
            {
                context.Result = GetUnauthorizedResult($"Some credentials is empty! [{username} | {idStr} | {password}])");
                return;
            }

            AppUser? user;
            if (long.TryParse(idStr, out var id))
            {
                user = await _appUserRepository.Get(id);
            }
            else
            {
                user = username.IsNotBlank()
                    ? await _appUserRepository.GetByUsername(username!)
                    : null;
            }

            if (user == null)
            {
                context.Result = GetUnauthorizedResult($"User with id '{id}' or username '{username}' not found!");
                return;
            }

            // For tests: '202CB962AC59075B964B07152D234B70' = '123'
            if (user.PasswordHash != StringExtensions.CreateMd5(password!))
            {
                context.Result = GetUnauthorizedResult($"Wrong password '{password}' for user with id '{id}' or username '{username}'!");
                return;
            }

            context.HttpContext.Request.Headers.Add("CurrentUserTgId", user.Id.ToString());
            context.HttpContext.Request.Headers.Add("CurrentUserTgUserName", user.Username);
            
            await next();
        }

        private static IActionResult GetUnauthorizedResult(string message)
        {
            return new ContentResult
            {
                StatusCode = StatusCodes.Status401Unauthorized,
                ContentType = "text/plain",
                Content = message
            };
        }
    }
}