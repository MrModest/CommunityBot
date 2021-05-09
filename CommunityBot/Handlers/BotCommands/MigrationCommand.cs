using System.Threading.Tasks;
using CommunityBot.Contracts;
using CommunityBot.Handlers.Results;
using CommunityBot.Persistence;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Telegram.Bot.Types;

namespace CommunityBot.Handlers.BotCommands
{
    public class MigrationCommand : BotCommandHandlerBase
    {
        private readonly MigrationRepository _migrationRepository;

        public MigrationCommand(
            IOptions<BotConfigurationOptions> options, 
            ILoggerFactory logger,
            MigrationRepository migrationRepository)
            : base(options, logger)
        {
            _migrationRepository = migrationRepository;
        }

        protected override BotCommandConfig Config { get; } = new("migration", isForAdmin: true, allowOnlyInPrivate: true, argRequiredMessage: "Введите SQL запрос для миграции!");
        protected override async Task<IUpdateHandlerResult> HandleUpdateInternal(Update update, string query)
        {
            Logger.LogWarning("Starting execute query: {Query}", query.Trim());

            var affectedRows = await _migrationRepository.ExecuteMigration(query);
            
            Logger.LogWarning("Completed execute query with affected {RowCount}: {Query}", affectedRows, query.Trim());

            return ReplyPlainText(update, $"Выполнение завершено. Зааффекчено строк: {affectedRows}.");
        }
    }
}
