using System;
using System.IO;
using System.Threading.Tasks;
using CommunityBot.Contracts;
using CommunityBot.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace CommunityBot.Controllers
{
    public class LogsController : ControllerBase
    {
        private readonly LoggingConfigurationOptions _loggingOptions;
        private readonly BotConfigurationOptions _botConfiguration;

        public LogsController(
            IOptions<LoggingConfigurationOptions> loggingOptions,
            IOptions<BotConfigurationOptions> botConfiguration)
        {
            _loggingOptions = loggingOptions.Value;
            _botConfiguration = botConfiguration.Value;
        }
        
        [HttpGet("/logs/{date?}")]
        public async Task<IActionResult> GetLogs(string? date = null)
        {
            if (_loggingOptions.LogDir.IsBlank())
            {
                return NotFound("LogDir not set!");
            }

            date ??= DateTime.Today.ToString("yyyyMMdd");
            
            var logPath = Path.Combine(_loggingOptions.LogDir, $"log-{date}.txt");

            if (!System.IO.File.Exists(logPath))
            {
                return NotFound($"Log file not found in {logPath}");
            }

            var logs = await System.IO.File.ReadAllLinesAsync(logPath);

            return Content(string.Join("\n________________________________________________\n", logs), "text/plain");
        }

        [HttpGet("/version")]
        public IActionResult GetVersion()
        {
            return Ok(_botConfiguration.Version);
        }
    }
}
