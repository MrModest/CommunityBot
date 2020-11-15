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
            return Ok($"Please, use POST methods! [{DateTime.Now.ToString("O")}]");
        }
        
        [HttpPost("api/web-hook")]
        public async Task<IActionResult> Post([FromBody]Update update)
        {
            await _botService.HandleUpdate(update);
            return Ok();
        }
        
        [HttpPost("api/set-webhook")]
        public async Task<IActionResult> SetWebhook()
        {
            await _botService.SetWebhook();
            return Ok();
        }
        
        [HttpPost("api/delete-webhook")]
        public async Task<IActionResult> DeleteWebhook()
        {
            await _botService.DeleteWebhook();
            return Ok();
        }
        
        [HttpGet("api/get-webhook-info")]
        public async Task<IActionResult> GetWebhookInfo()
        {
            var info = await _botService.GetWebhookInfo();
            return Ok(info);
        }
        
        [HttpPost("api/start-polling")]
        public async Task<IActionResult> StartPolling()
        {
            await _botService.StartPolling();
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