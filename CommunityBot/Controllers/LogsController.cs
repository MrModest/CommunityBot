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

        public LogsController(
            IOptions<LoggingConfigurationOptions> loggingOptions)
        {
            _loggingOptions = loggingOptions.Value;
        }
        
        [HttpGet("/logs/{date}")]
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

            return Content(string.Join("________________________________________________\n", logs), "text/plain");
        }
        
        [HttpDelete("/logs")]
        public async Task<IActionResult> DeleteLogFile()
        {
            if (_loggingOptions.FilePath.IsBlank())
            {
                return NotFound("Log file not set!");
            }

            if (!System.IO.File.Exists(_loggingOptions.FilePath))
            {
                return NotFound($"Log file not found in {_loggingOptions.FilePath}");
            }

            System.IO.File.Delete(_loggingOptions.FilePath);
            
            if (!System.IO.File.Exists(_loggingOptions.FilePath))
            {
                return Ok($"File in {_loggingOptions.FilePath} was deleted!");
            }

            return Ok($"File in {_loggingOptions.FilePath} was NOT deleted!");
        }
    }
}
