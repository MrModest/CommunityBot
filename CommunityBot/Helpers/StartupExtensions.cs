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
                .AddSingleton<IMediaGroupService, MediaGroupService>()
                .AddSingleton<InMemorySettingsService>();
        }
        
        public static IServiceCollection AddSqliteDatabase(this IServiceCollection services)
        {
            return services.AddSingleton(provider =>
            {
                var options = provider.GetRequiredService<IOptions<SQLiteConfigurationOptions>>().Value;
                
                var connection = new SQLiteConnection($"DataSource=\"{options.DbFilePath}\";");
                connection.Open();
                
                EnsureDatabase(connection);
                
                return connection;
            });
        }

        public static void ConfigureSerilog(HostBuilderContext hostBuilderContext, ILoggingBuilder loggingBuilder)
        {    
            var configuration = hostBuilderContext.Configuration;
            var loggerConfiguration = new LoggerConfiguration();
            
            /*loggerConfiguration = loggerConfiguration
                .WriteTo
                .File(configuration.GetSection("Logging:FilePath").Value);*/

            var pathFormat = Path.Combine(configuration.GetSection("Logging:LogDir").Value, "log-{Date}.txt");

            loggerConfiguration = loggerConfiguration
                .WriteTo
                .RollingFile(pathFormat);

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

        private static void EnsureDatabase(SQLiteConnection connection)
        {
            var appTableNames = new[] {"SavedChats", "Users"};
            
            var tableNames = connection.Query<string>("SELECT name FROM sqlite_master WHERE type='table';");

            foreach (var tableName in appTableNames.Except(tableNames))
            {
                if (tableName == "SavedChats")
                {
                    connection.Execute(@"CREATE TABLE IF NOT EXISTS main.SavedChats (
                                            Id        INT  NOT NULL PRIMARY KEY, 
                                            ChatId    INT  NOT NULL, 
                                            ExactName TEXT NOT NULL, 
                                            JoinLink  TEXT NOT NULL);");
                    connection.Execute("CREATE INDEX IF NOT EXISTS main.ExactName_desc ON SavedChats (ExactName DESC);");
                }

                if (tableName == "Users")
                {
                    connection.Execute(@"CREATE TABLE IF NOT EXISTS main.Users (
                                            Id            INT  NOT NULL PRIMARY KEY, 
                                            Username      TEXT DEFAULT NULL, 
                                            FirstName     TEXT DEFAULT NULL, 
                                            LastName      TEXT DEFAULT NULL, 
                                            InvitedBy     INT  DEFAULT NULL, 
                                            InviteComment TEXT DEFAULT NULL, 
                                            AccessType    TEXT NOT NULL,
                                            PasswordHash  TEXT DEFAULT NULL);");
                    
                    connection.Execute("CREATE INDEX IF NOT EXISTS main.Username_desc   ON Users (Username DESC);");
                    connection.Execute("CREATE INDEX IF NOT EXISTS main.AccessType_desc ON Users (AccessType DESC);");
                }
            }
        }
    }
}