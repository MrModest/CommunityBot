using System.Data.SQLite;
using System.Linq;
using CommunityBot.Contracts;
using CommunityBot.Handlers;
using CommunityBot.Persistence;
using CommunityBot.Services;
using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Serilog;
using Serilog.Events;
using Telegram.Bot;
using TelegramSink;

namespace CommunityBot.Helpers
{
    public static class StartupExtensions
    {
        public static IServiceCollection AddTelegramBotClient(this IServiceCollection services)
        {
            return services.AddSingleton<ITelegramBotClient>(provider =>
            {
                var options = provider.GetService<IOptions<BotConfigurationOptions>>().Value;
                
                return new TelegramBotClient(options.BotToken);
            });
        }

        public static IServiceCollection AddUpdateHandlers(this IServiceCollection services)
        {
            return services
                .AddSingleton<IUpdateHandler, RepostMessageUpdateHandler>()
                .AddSingleton<IUpdateHandler, InfoMessageUpdateHandler>()
                .AddSingleton<IUpdateHandler, ChatManageUpdateHandler>()
                .AddSingleton<IUpdateHandler, MediaGroupUpdateHandler>();
        }
        
        public static IServiceCollection AddServices(this IServiceCollection services)
        {
            return services
                .AddSingleton<BotService>()
                .AddSingleton<IChatRepository, SqliteChatRepository>()
                .AddSingleton<IMediaGroupService, MediaGroupService>();
        }
        
        public static IServiceCollection AddSqliteDatabase(this IServiceCollection services)
        {
            return services.AddSingleton(provider =>
            {
                var options = provider.GetService<IOptions<SQLiteConfigurationOptions>>().Value;
                
                var connection = new SQLiteConnection($"DataSource=\"{options.DbFilePath}\";");
                connection.Open();
                
                EnsureDatabase(connection);
                
                return connection;
            });
        }

        public static IServiceCollection AddLogger(this IServiceCollection services)
        {
            return services.AddTransient<ILogger>(provider =>
            {
                var configuration = provider.GetService<IConfiguration>();
                var botConfigurationOptions = provider.GetService<IOptions<BotConfigurationOptions>>();
                
                var loggerConfiguration = new LoggerConfiguration();
                var config = loggerConfiguration.WriteTo.File(configuration.GetSection("Logging:FilePath").Value);

                foreach (var debugInfoChatId in botConfigurationOptions.Value.DebugInfoChatIds)
                {
                    config = config.WriteTo.TeleSink(
                        botConfigurationOptions.Value.BotToken,
                        debugInfoChatId.ToString(),
                        minimumLevel: LogEventLevel.Warning);
                }
                return config.CreateLogger();
            });
        }

        private static void EnsureDatabase(SQLiteConnection connection)
        {
            var appTableNames = new[] {"SavedChats"};
            
            var tableNames = connection.Query<string>("SELECT name FROM sqlite_master WHERE type='table';");

            foreach (var tableName in appTableNames.Except(tableNames))
            {
                if (tableName == "SavedChats")
                {
                    connection.Execute("CREATE TABLE IF NOT EXISTS main.SavedChats (Id INT PRIMARY KEY, ChatId INT DEFAULT NULL, ExactName TEXT NOT NULL, JoinLink TEXT DEFAULT NULL);");
                    connection.Execute("CREATE INDEX IF NOT EXISTS main.ExactName_desc ON SavedChats (ExactName DESC);");
                }
            }
        }
    }
}