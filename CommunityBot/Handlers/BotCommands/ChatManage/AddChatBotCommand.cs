using System;
using System.Threading.Tasks;
using CommunityBot.Contracts;
using CommunityBot.Handlers.Results;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace CommunityBot.Handlers.BotCommands.ChatManage
{
    public class AddChatBotCommand : BotCommandHandlerBase
    {
        private readonly IChatRepository _chatRepository;

        private const string HelpMessage = "Чтобы добавить ссылку на чат, отправь сообщение в формате: " +
                                            "<pre>/add_chat\nПолное название чата\nСсылку-приглашение на чат</pre>" +
                                            "И название и ссылку обязательно с новой строчки." +
                                            "Также важно, чтобы полное название чата было один в один с реальным названием, для этого его можно скопировать из информации о чате.";

        public AddChatBotCommand(
            IOptions<BotConfigurationOptions> options,
            IChatRepository chatRepository,
            ILoggerFactory logger)
            : base(options, logger)
        {
            _chatRepository = chatRepository;
        }

        protected override BotCommandConfig Config { get; } =
            new ("add_chat", HelpMessage);
        protected override async Task<IUpdateHandlerResult> HandleUpdateInternal(Update update, string chatRawArgs)
        {
            var arg = chatRawArgs.Split('\n', StringSplitOptions.RemoveEmptyEntries);

            if (arg.Length < 2)
            {
                return Result.Text(
                    update.Message.Chat.Id, 
                    "Неправильно отправленная команда. Пожалуйста попробуйте ещё раз или обратитесь к админам за помощью.", 
                    update.Message.MessageId);
            }

            if (!arg[1].StartsWith("https://t.me/joinchat/") && !arg[1].StartsWith("https://t.me/+"))
            {
                return Result.Text(
                    update.Message.Chat.Id,
                    "Неправильная ссылка приглашение: ссылка должна начинаться с 'https://t.me/joinchat/' или с 'https://t.me/+'. Добавлять ссылку на публичные чаты не нужно.",
                    update.Message.MessageId);
            }
                
            var chatName = arg[0];
            var chatLink = arg[1];

            var isUpdated = await _chatRepository.AddOrUpdate(new SavedChat(-1, chatName, chatLink));
            var text = $"Ссылка на чат {(isUpdated ? "обновлена" : "добавлена")}! Спасибо за помощь боту!";

            return Result.Text(update.Message.Chat.Id, text, update.Message.MessageId);
        }
    }
}
