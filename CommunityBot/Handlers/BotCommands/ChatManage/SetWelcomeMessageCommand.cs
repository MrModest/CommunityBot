using System.Threading.Tasks;
using CommunityBot.Contracts;
using CommunityBot.Handlers.Results;
using CommunityBot.Helpers;
using CommunityBot.Persistence;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace CommunityBot.Handlers.BotCommands.ChatManage
{
    public class SetWelcomeMessageCommand : BotCommandHandlerBase
    {
        private readonly IChatRepository _savedChatRepository;

        public SetWelcomeMessageCommand(
            IOptions<BotConfigurationOptions> options, 
            ILoggerFactory logger,
            IChatRepository savedChatRepository)
            : base(options, logger)
        {
            _savedChatRepository = savedChatRepository;
        }

        protected override BotCommandConfig Config { get; } = new("set_welcome", isForAdmin: true,
            argRequiredMessage: "Введите текст приветственного сообщения. " +
                                "Если хотите отобразить кнопку, добавьте с новой строки три черты '---'. " +
                                "Затем снова с новой строки опишите кнопку в формате 'текст кнопки | https:\\\\ссылка.ру'" +
                                "Если надо выключить приветственные сообщения, то просто напишите '/set_welcome off'.");
        protected override async Task<IUpdateHandlerResult> HandleUpdateInternal(Update update, string welcomeRaw)
        {
            if (update.Message.IsPrivate())
            {
                return ReplyPlainText(update, "Команду необходимо вводить в том чате, в котором редактируется приветственно сообщение!");
            }
            
            var message = Parse(welcomeRaw);

            var savedChat = await _savedChatRepository.GetByName(update.Message.Chat.Title)
                            ?? new SavedChat(update.Message.Chat.Id, update.Message.Chat.Title, "---");

            savedChat.ChatId = update.Message.Chat.Id;
            savedChat.WelcomeMessage = JsonConvert.SerializeObject(message);

            await _savedChatRepository.AddOrUpdate(savedChat);
            
            var reply = ReplyPlainText(update, "Настройки сохранены. Следующим сообщением придёт пример приветственного сообщения");

            var exampleResult = Result.Text(update.Message.Chat.Id, $"@username\n\n{message.Message}",
                update.Message.MessageId, ParseMode.Html, false, InlineKeyboardHelper.GetWelcomeButton(message));

            return Result.Inners(new[] {reply, exampleResult});
        }

        private static WelcomeMessage Parse(string welcomeRaw)
        {
            var split = welcomeRaw.Split("---");

            if (split.Length != 3)
            {
                return new WelcomeMessage(true, split[0].Trim(), "", "");
            }

            return new WelcomeMessage(true, split[0].Trim(), split[1].Trim(), split[2].Trim());
        }
    }
}
