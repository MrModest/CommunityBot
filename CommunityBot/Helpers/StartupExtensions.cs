using System.Data.SQLite;
using System.IO;
using System.Linq;
using CommunityBot.Contracts;
using CommunityBot.Handlers;
using CommunityBot.Persistence;
using CommunityBot.Services;
using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Serilog;
using Serilog.Events;
using Telegram.Bot;
using TelegramSink;

namespace CommunityBot.Helpers
{
    public static class StartupExtensions
    {
        public static IServiceCollection AddConfigurationOptions(this IServiceCollection services, IConfiguration configuration)
        {
            return services
                .Configure<BotConfigurationOptions>(configuration.GetSection(BotConfigurationOptions.SectionName))
                .Configure<SQLiteConfigurationOptions>(configuration.GetSection(SQLiteConfigurationOptions.SectionName))
                .Configure<LoggingConfigurationOptions>(configuration.GetSection(LoggingConfigurationOptions.SectionName));
        }
        
        public static IServiceCollection AddTelegramBotClient(this IServiceCollection services)
        {
            return services.AddSingleton<ITelegramBotClient>(provider =>
            {
                var options = provider.GetRequiredService<IOptions<BotConfigurationOptions>>().Value;
                
                return new TelegramBotClient(options.BotToken);
            });
        }

        public static IServiceCollection AddUpdateHandlers(this IServiceCollection services)
        {
            return services
                .AddSingleton<IUpdateHandler, RepostMessageUpdateHandler>()
                .AddSingleton<IUpdateHandler, InfoMessageUpdateHandler>()
                .AddSingleton<IUpdateHandler, ChatManageUpdateHandler>()
                .AddSingleton<IUpdateHandler, MediaGroupUpdateHandler>()
                .AddSingleton<IUpdateHandler, BackupUpdateHandler>()
                .AddSingleton<IUpdateHandler, UserDataCollectorUpdateHandler>()
                .AddSingleton<IUpdateHandler, UserUpdateHandler>();
        }
        
        public static IServiceCollection AddServices(this IServiceCollection services)
        {
            return services
                .AddSingleton<BotService>()
                .AddSingleton<IChatRepository, SqliteChatRepository>()
                .AddSingleton<IAppUserRepository, AppUserRepository>()
                .AddSingleton<ILogRepository, LogRepository>()
                .AddSingleton<IMediaGroupService, MediaGroupService>()
                .AddSingleton<InMemorySettingsService>();
        }

        public static void ConfigureSerilog(HostBuilderContext hostBuilderContext, ILoggingBuilder loggingBuilder)
        {    
            var configuration = hostBuilderContext.Configuration;
            
            var logDir = configuration.GetSection("Logging:LogDir").Value;

            var loggerConfiguration = new LoggerConfiguration()
                .WriteTo
                .RollingFile(Path.Combine(logDir, "log-{Date}.txt"));
            
            
            var logDbFilePath = configuration.GetSection("SQLite:LogDbFilePath").Value;

            loggerConfiguration = loggerConfiguration
                .WriteTo
                .SQLite(logDbFilePath);
            

            var telegramApiKey = configuration.GetSection("BotConfiguration:BotToken").Value;
            var debugInfoChatIds = configuration.GetSection("BotConfiguration:DebugInfoChatIds").Get<long[]>();
            foreach (var debugInfoChatId in debugInfoChatIds)
            {
                loggerConfiguration = loggerConfiguration.WriteTo.TeleSink(
                    telegramApiKey,
                    debugInfoChatId.ToString(),
                    minimumLevel: LogEventLevel.Warning);
            }

            loggingBuilder.AddSerilog(loggerConfiguration.CreateLogger());
        }

        
    }
}