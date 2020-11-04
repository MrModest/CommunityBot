using System.Data.SQLite;
using CommunityBot.Contracts;
using CommunityBot.Handlers;
using CommunityBot.Persistence;
using CommunityBot.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Telegram.Bot;

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
                .AddSingleton<IUpdateHandler, ChatManageUpdateHandler>();
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
                return connection;
            });
        }
    }
}