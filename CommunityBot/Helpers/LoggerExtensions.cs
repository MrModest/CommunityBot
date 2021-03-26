using System;
using Microsoft.Extensions.Logging;

namespace CommunityBot.Helpers
{
    public static class LoggerExtensions
    {
        public static IDisposable AddContext(this ILogger logger, string fieldName, object fieldValue)
        {
            var format = $"{fieldName}: {{{fieldName}}}";
            return logger.BeginScope(format, fieldValue);
        }
    }
}