using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Telegram.Bot;
using Telegram.Bot.Types;

using CommunityBot.Contracts;
using CommunityBot.Handlers.Results;
using CommunityBot.Helpers;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;
using File = System.IO.File;

namespace CommunityBot.Handlers
{
    public class BackupUpdateHandler : UpdateHandlerBase
    {
        private readonly SQLiteConfigurationOptions _dbOptions;
        private const string BackupDbCommand = "backup_db";
        
        public BackupUpdateHandler(
            ITelegramBotClient botClient, 
            IOptions<BotConfigurationOptions> options,
            IOptions<SQLiteConfigurationOptions> dbOptions,
            ILogger<BackupUpdateHandler> logger)
            : base(botClient, options, logger)
        {
            _dbOptions = dbOptions.Value;
        }

        protected override UpdateType[] AllowedUpdates => new[] {UpdateType.Message};

        protected override bool CanHandle(Update update)
        {
            return update.Message.ContainCommand(BackupDbCommand);
        }

        protected override async Task<IUpdateHandlerResult> HandleUpdateInternalAsync(Update update)
        {
            if (!IsFromAdmin(update))
            {
                return Result.FromText(update.Message.Chat.Id, "Эта команда только для админов!", update.Message.MessageId);
            }

            if (!File.Exists(_dbOptions.DbFilePath))
            {
                return Result.FromText(update.Message.Chat.Id, $"Не найден файл БД по пути '{_dbOptions.DbFilePath}'!", update.Message.MessageId);
            }

            var stream = File.Open(_dbOptions.DbFilePath, FileMode.Open);

            var fileName = $"db_{DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss_zz").Replace(" ", "_")}.sqlite";

            var results = Options.DebugInfoChatIds.Select(chatId =>
                Result.FromDocument(chatId, stream, fileName, "#backup"));

            return Result.FromInner(results);
        }
    }
}