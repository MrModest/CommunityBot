using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using CommunityBot.Contracts;
using CommunityBot.Helpers;
using CommunityBot.Middleware;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

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

            foreach (var user in users)
            {
                user.PasswordHash = "<hidden>";
            }

            var isAdminString = HttpContext.Request.Headers["CurrentUserIsAdmin"].FirstOrDefault();
            var isAdmin = bool.TryParse(isAdminString, out var v) && v;

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
            if (viewName == null)
            {
                throw new ArgumentNullException(nameof(viewName));
            }
        
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = $"{Path.GetFileNameWithoutExtension(assembly.ManifestModule.Name)}.ViewTemplates.{viewName}.html";
            var htmlTemplateStream = assembly.GetManifestResourceStream(resourceName);
            
            if (htmlTemplateStream == null)
            {
                throw new ArgumentException($"The specified embedded resource {resourceName} is not found.");
            }

            var htmlTemplate = await new StreamReader(htmlTemplateStream).ReadToEndAsync();

            var json = JsonConvert.SerializeObject(model);
            
            var htmlResult = htmlTemplate.Replace("{%model%}", json);

            return Content(string.Join("<hr />\n", htmlResult), "text/html");
        }
    }
}
