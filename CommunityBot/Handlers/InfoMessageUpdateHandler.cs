using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using CommunityBot.Contracts;
using CommunityBot.Handlers.Results;
using CommunityBot.Helpers;
using Microsoft.Extensions.Logging;

namespace CommunityBot.Handlers
{
    public class InfoMessageUpdateHandler : UpdateHandlerBase
    {
        private const string _helpText = "Добавь меня в свою группу, чтобы пользователи могли упоминать меня.\n" +
                                         "Если упомянуть меня в ответе на сообщение, я отправлю пост в \"Секретные Движухи\". \n" +
                                         "Отправь `/event Описание события`, чтобы запостить что-то сразу в канал.\n" +
                                         "Добавить ссылку на чат: /add_chat";
        
        private static Dictionary<string, string> infoDict = new Dictionary<string, string>
        {
            ["help"] = _helpText,
            ["start"] = "Добро пожаловать, Друже!\n" + _helpText,
            ["event"] = "Пример использования:\n`/event Всем привет. Завтра тестовое событие в 13-00`",
            ["event-success"] = "Ваше событие успешно запосчено!",
            ["add_chat"] = "Чтобы добавить ссылку на чат, отправь сообщение в формате: " +
                           "`/add_chat\nПолное название чата\nСсылку-приглашение на чат`" +
                           "И название и ссылку обязательно с новой строчки." + 
                           "Также важно, чтобы полное название чата было один в один с реальным названием, для этого его можно скопировать из информации о чате."
        };
        
        
        public InfoMessageUpdateHandler(
            ITelegramBotClient botClient,
            IOptions<BotConfigurationOptions> options,
            ILogger<InfoMessageUpdateHandler> logger) 
            : base(botClient, options, logger)
        {
        }
        
        protected override UpdateType[] AllowedUpdates => new[] {UpdateType.Message};

        protected override bool CanHandle(Update update)
        {
            var command = update.Message.GetFirstBotCommand();
            return command.HasValue &&
                   infoDict.Keys.Contains(command.Value.name) &&
                   command.Value.arg.IsBlank();
        }

        //ToDo: may be need change concept (/help <commandName>) and don't touch command with empty args
        protected override async Task<IUpdateHandlerResult> HandleUpdateInternalAsync(Update update)
        {
            var command = update.Message.GetFirstBotCommand();

            if (infoDict.TryGetValue(command?.name ?? string.Empty, out var text))
            {
                return new TextUpdateHandlerResult(update.Message.Chat.Id, text, update.Message.MessageId, ParseMode.Html);
            }
                
            Logger.LogInformation("Command {CommandName} was skipped because not found in infoDict", command?.name);

            return new NothingUpdateHandlerResult();
        }
    }
}