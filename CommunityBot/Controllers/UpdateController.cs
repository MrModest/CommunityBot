using System;
using System.Threading.Tasks;
using CommunityBot.Services;
using Microsoft.AspNetCore.Mvc;
using Telegram.Bot.Types;

namespace CommunityBot.Controllers
{
    public class UpdateController : ControllerBase
    {
        private readonly BotService _botService;

        public UpdateController(BotService botService)
        {
            _botService = botService;
        }

        [HttpGet("")]
        public IActionResult Get()
        {
            return Ok("Please, use POST methods!");
        }
        
        [HttpPost("api/web-hook")]
        public async Task<IActionResult> Post([FromBody]Update update)
        {
            await _botService.HandleUpdate(update);
            return Ok();
        }
        
        [HttpPost("api/start-polling")]
        public async Task<IActionResult> StartPolling(int? timeoutMinute = null)
        {
            await _botService.StartPolling(timeoutMinute.HasValue
                ? TimeSpan.FromMinutes(timeoutMinute.Value)
                : (TimeSpan?)null);
            return Ok();
        }
        
        [HttpPost("api/stop-polling")]
        public IActionResult StopPolling()
        { 
            _botService.StopPolling();
            return Ok();
        }
    }
}