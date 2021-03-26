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
        private readonly ILogRepository _logRepository;
        private readonly LoggingConfigurationOptions _loggingOptions;

        public LogsController(
            IOptions<LoggingConfigurationOptions> loggingOptions,
            ILogRepository logRepository)
        {
            _logRepository = logRepository;
            _loggingOptions = loggingOptions.Value;
        }
        
        [HttpGet("/logs/{date?}")]
        public async Task<IActionResult> GetLogs(string? date = null)
        {
            var logs = await _logRepository.ByDate();

            return Ok(logs);
        }
    }
}
