using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

using CommunityBot.Contracts;
using CommunityBot.Helpers;

namespace CommunityBot.Handlers
{
    public class InfoMessageUpdateHandler : UpdateHandlerBase
    {
        private static Dictionary<string, string> infoDict = new Dictionary<string, string>
        {
            ["help"] = "Добавь меня в свою группу, чтобы пользователи могли упоминать меня.\n" +
                       "Если упомянуть меня в ответе на сообщение, я отправлю пост в \"Секретные Движухи\". \n" +
                       "Отправь `/event Описание события`, чтобы запостить что-то сразу в канал.\n" +
                       "Добавить ссылку на чат: /add_chat",
            
            ["start"] = "Добро пожаловать, Друже!\n" + infoDict["help"],
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
            ILogger<UpdateHandlerBase> logger) 
            : base(botClient, options, logger)
        {
        }
        
        protected override UpdateType[] AllowedUpdates => new[] {UpdateType.Message};

        protected override bool CanHandle(Update update)
        {
            var command = update.Message.GetFirstBotCommand();
            return command.HasValue &&
                   infoDict.Keys.Contains(command.Value.name) &&
                   string.IsNullOrWhiteSpace(command.Value.arg);
        }

        protected override async Task HandleUpdateInternalAsync(Update update)
        {
            var command = update.Message.GetFirstBotCommand();

            if (infoDict.TryGetValue(command?.name ?? string.Empty, out var text))
            {
                await BotClient.SendTextMessageAsync(Options.MainChannelId, text, ParseMode.MarkdownV2);
                return;
            }
                
            Logger.LogInformation($"Command {command?.name} was skipped because not found in infoDict.");
        }
    }
}