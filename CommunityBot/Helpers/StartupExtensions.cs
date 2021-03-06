using System.Data.SQLite;
using System.IO;
using System.Linq;
using CommunityBot.Contracts;
using CommunityBot.Handlers;
using CommunityBot.Handlers.BotCommands;
using CommunityBot.Handlers.BotCommands.ChatManage;
using CommunityBot.Handlers.BotCommands.User;
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

                .AddSingleton<IUpdateHandler, AddChatBotCommand>()
                .AddSingleton<IUpdateHandler, AddThisChatBotCommand>()
                .AddSingleton<IUpdateHandler, RemoveChatBotCommand>()
                .AddSingleton<IUpdateHandler, GetAllChatsBotCommand>()
                .AddSingleton<IUpdateHandler, GetIdOfThisChatBotCommand>()
                .AddSingleton<IUpdateHandler, SetWelcomeMessageCommand>()

                .AddSingleton<IUpdateHandler, BackupCommand>()
                .AddSingleton<IUpdateHandler, GetVersionCommand>()

                .AddSingleton<IUpdateHandler, SetPasswordCommand>()
                .AddSingleton<IUpdateHandler, InMemorySettingsToggleCommand>()
                .AddSingleton<IUpdateHandler, AddUsersFromJsonCommand>()
                .AddSingleton<IUpdateHandler, ShowAdminsCommand>()
                .AddSingleton<IUpdateHandler, MigrationCommand>()

                .AddSingleton<IUpdateHandler, MediaGroupUpdateHandler>()
                .AddSingleton<IUpdateHandler, NewUsersCheckUpdateHandler>()
                .AddSingleton<IUpdateHandler, WelcomeMessageUpdateHandler>();
        }
        
        public static IServiceCollection AddServices(this IServiceCollection services)
        {
            return services
                .AddSingleton<BotService>()
                .AddSingleton<IChatRepository, SqliteChatRepository>()
                .AddSingleton<IAppUserRepository, AppUserRepository>()
                .AddSingleton<MigrationRepository>()
                .AddSingleton<IMediaGroupService, MediaGroupService>()
                .AddSingleton<IMemoryCacheWrapperFactory, MemoryCacheWrapperFactory>()
                .AddSingleton<InMemorySettingsService>();
        }
        
        public static IServiceCollection AddSqliteDatabase(this IServiceCollection services)
        {
            return services.AddSingleton(provider =>
            {
                var options = provider.GetRequiredService<IOptions<SQLiteConfigurationOptions>>().Value;

                var path = Path.GetDirectoryName(options.DbFilePath);

                Directory.CreateDirectory(path);
                
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
                                            Id        INTEGER  NOT NULL PRIMARY KEY AUTOINCREMENT, 
                                            ChatId    INTEGER  NOT NULL, 
                                            ExactName TEXT     NOT NULL, 
                                            JoinLink  TEXT     NOT NULL);");
                    connection.Execute("CREATE INDEX IF NOT EXISTS main.ExactName_desc ON SavedChats (ExactName DESC);");
                }

                if (tableName == "Users")
                {
                    connection.Execute(@"CREATE TABLE IF NOT EXISTS main.Users (
                                            Id            INTEGER NOT NULL PRIMARY KEY, 
                                            Username      TEXT    DEFAULT NULL, 
                                            FirstName     TEXT    DEFAULT NULL, 
                                            LastName      TEXT    DEFAULT NULL, 
                                            Joined        TEXT    DEFAULT '2000-01-01 00:00:00',
                                            InvitedBy     INT     DEFAULT NULL, 
                                            InviteComment TEXT    DEFAULT NULL, 
                                            AccessType    INTEGER NOT NULL,
                                            PasswordHash  TEXT    DEFAULT NULL);");
                    
                    connection.Execute("CREATE INDEX IF NOT EXISTS main.Username_desc   ON Users (Username DESC);");
                    connection.Execute("CREATE INDEX IF NOT EXISTS main.AccessType_desc ON Users (AccessType DESC);");
                }
            }
        }
    }
}
