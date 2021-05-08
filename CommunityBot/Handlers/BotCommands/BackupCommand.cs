using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CommunityBot.Contracts;
using CommunityBot.Handlers.Results;
using CommunityBot.Helpers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Types;
using File = System.IO.File;

namespace CommunityBot.Handlers.BotCommands
{
    public class BackupCommand : BotCommandHandlerBase
    {
        private readonly SQLiteConfigurationOptions _dbOptions;

        public BackupCommand( 
            IOptions<BotConfigurationOptions> options, 
            ILoggerFactory logger,
            IOptions<SQLiteConfigurationOptions> dbOptions)
            : base(options, logger)
        {
            _dbOptions = dbOptions.Value;
        }

        protected override BotCommandConfig Config { get; } = new("backup_db", isForAdmin: true);
        protected override Task<IUpdateHandlerResult> HandleUpdateInternal(Update update, string commandArg)
        {
            if (!File.Exists(_dbOptions.DbFilePath))
            {
                return Result.Text(update.Message.Chat.Id, $"Не найден файл БД по пути '{_dbOptions.DbFilePath}'!", update.Message.MessageId).AsTask();
            }

            var stream = File.Open(_dbOptions.DbFilePath, FileMode.Open);

            var fileName = $"db_{DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss_zz").Replace(" ", "_")}.sqlite";

            var results = Options.DebugInfoChatIds.Select(chatId =>
                Result.Document(chatId, stream, fileName, "#backup"));

            return Result.Inners(results).AsTask();
        }
    }
}
