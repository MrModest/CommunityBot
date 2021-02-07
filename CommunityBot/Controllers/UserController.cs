using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using CommunityBot.Contracts;
using CommunityBot.Helpers;
using CommunityBot.Middleware;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace CommunityBot.Controllers
{
    [TypeFilter(typeof(TgAuthorizationFilter))]
    public class UserController : ControllerBase
    {
        private readonly IAppUserRepository _appUserRepository;

        public UserController(
            IAppUserRepository appUserRepository)
        {
            _appUserRepository = appUserRepository;
        }

        [HttpGet("/api/users")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _appUserRepository.GetAll();

            foreach (var user in users)
            {
                user.PasswordHash = "<hidden>";
            }

            return Ok(users);
        }
        
        [HttpGet("/api/users-list")]
        public async Task<IActionResult> GetAllUserList()
        {
            var users = (await _appUserRepository.GetAll())
                .ToArray();

            var isAdminString = HttpContext.Request.Headers["CurrentUserIsAdmin"].FirstOrDefault();
            var isAdmin = bool.TryParse(isAdminString, out var v) && v;

            if (!isAdmin)
            {
                foreach (var user in users)
                {
                    user.PasswordHash = "<hidden>";
                }
            }

            return await GetHtmlView("UserList", new
            {
                IsAdmin = isAdmin,
                Users = users
            });
        }
        
        [HttpPost("/api/users/add")]
        [TypeFilter(typeof(TgAdminOnly))]
        public async Task<IActionResult> AddUsers([FromBody] AppUser[] appUsers)
        {
            var skippedUsers = appUsers.Where(u => u.Id <= 0 || (u.Username.IsBlank() && u.FirstName.IsBlank())).ToArray();

            foreach (var appUser in appUsers.Except(skippedUsers))
            {
                await _appUserRepository.Add(appUser);
            }

            return Ok(new
            {
                added = appUsers.Except(skippedUsers).Select(u => u.ToString()).ToArray(),
                skipped = skippedUsers.Select(u => u.ToString()).ToArray()
            });
        }

        private async Task<IActionResult> GetHtmlView(string viewName, object model)
        {
            var htmlResult = await GetHtmlString(viewName, model);

            return Content(string.Join("<hr />\n", htmlResult), "text/html");
        }

        private static async Task<string> GetHtmlString(string viewName, object model)
        {
            if (viewName == null)
            {
                throw new ArgumentNullException(nameof(viewName));
            }
        
            var assemblyLocation = Assembly.GetExecutingAssembly().Location;
            var projectDir = Path.GetDirectoryName(assemblyLocation);

            if (projectDir == null)
            {
                throw new IOException($"Can't get projectDirectory. AssemblyLocation: '{assemblyLocation}'");
            }
            
            var path = Path.Combine(projectDir, $"ViewTemplates\\{viewName}.html");

            var json = JsonConvert.SerializeObject(model, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });
            var htmlTemplate = await System.IO.File.ReadAllTextAsync(path);
            
            return htmlTemplate.Replace("{%model%}", json);
        }
    }
}
