using System;
using System.Threading.Tasks;
using CommunityBot.Contracts;
using CommunityBot.Helpers;
using CommunityBot.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Telegram.Bot.Types;

namespace CommunityBot.Controllers
{
    public class UpdateController : ControllerBase
    {
        private readonly LoggingConfigurationOptions _loggingOptions;
        private readonly BotService _botService;

        public UpdateController(
            IOptions<LoggingConfigurationOptions> loggingOptions,
            BotService botService)
        {
            _loggingOptions = loggingOptions.Value;
            _botService = botService;
        }

        [HttpGet("")]
        public IActionResult Get()
        {
            return Ok($"Please, use POST methods! [{DateTime.Now.ToString("O")}]");
        }

        [HttpGet("/logs")]
        public async Task<IActionResult> GetLogs()
        {
            if (_loggingOptions.FilePath.IsBlank())
            {
                return NotFound("Log file not set!");
            }

            if (!System.IO.File.Exists(_loggingOptions.FilePath))
            {
                return NotFound($"Log file not found in {_loggingOptions.FilePath}");
            }

            var logs = await System.IO.File.ReadAllLinesAsync(_loggingOptions.FilePath);

            return Content(string.Join("<hr />\n", logs), "text/html");
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