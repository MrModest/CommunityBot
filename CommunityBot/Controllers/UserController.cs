using System.Linq;
using System.Threading.Tasks;
using CommunityBot.Contracts;
using CommunityBot.Helpers;
using CommunityBot.Middleware;
using Microsoft.AspNetCore.Mvc;

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

            return Ok(users);
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
    }
}