using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Telegram.Bot;
using Telegram.Bot.Types;

using CommunityBot.Contracts;
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
        private const string VersionDbCommand = "version";
        
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
            return update.Message.ContainCommand(BackupDbCommand, VersionDbCommand);
        }

        protected override async Task HandleUpdateInternalAsync(Update update)
        {
            var command = update.Message.GetFirstBotCommand()!.Value;

            switch (command.name)
            {
                case BackupDbCommand:
                    await Backup(update);
                    return;
                
                case VersionDbCommand:
                    await SendMessage(update.Message.Chat.Id, Options.Version, update.Message.MessageId);
                    return;
            }
        }

        private async Task Backup(Update update)
        {
            if (!IsFromAdmin(update))
            {
                await SendMessage(update.Message.Chat.Id, "Эта команда только для админов!", update.Message.MessageId);
                return;
            }

            if (!File.Exists(_dbOptions.DbFilePath))
            {
                await SendMessage(update.Message.Chat.Id, $"Не найден файл БД по пути '{_dbOptions.DbFilePath}'!", update.Message.MessageId);
                return;
            }

            await using var stream = File.Open(_dbOptions.DbFilePath, FileMode.Open);
            InputOnlineFile iof = new InputOnlineFile(stream) {FileName = $"db_{DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss_zz").Replace(" ", "_")}.sqlite"};
                
            foreach (var debugInfoChatId in Options.DebugInfoChatIds)
            {
                await BotClient.SendDocumentAsync(debugInfoChatId, iof, "#backup");
            }
        }
        
        private async Task SendMessage(long replyChatId, string text, int replyToMessageId)
        {
            await BotClient.SendTextMessageAsync(replyChatId, text, replyToMessageId: replyToMessageId);
        }
    }
}
