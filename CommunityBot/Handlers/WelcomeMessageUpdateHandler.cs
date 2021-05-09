using System.Linq;
using System.Threading.Tasks;
using CommunityBot.Contracts;
using CommunityBot.Handlers.Results;
using CommunityBot.Helpers;
using CommunityBot.Persistence;
using CommunityBot.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace CommunityBot.Handlers
{
    public class WelcomeMessageUpdateHandler : UpdateHandlerBase

    {
        private readonly InMemorySettingsService _inMemorySettingsService;
        private readonly IChatRepository _savedChatRepository;

        public WelcomeMessageUpdateHandler(
            IOptions<BotConfigurationOptions> options, 
            ILoggerFactory loggerFactory,
            InMemorySettingsService inMemorySettingsService,
            IChatRepository savedChatRepository)
            : base(options, loggerFactory)
        {
            _inMemorySettingsService = inMemorySettingsService;
            _savedChatRepository = savedChatRepository;
        }

        protected override UpdateType[] AllowedUpdates => new[] {UpdateType.Message};
        protected override bool CanHandle(Update update)
        {
            return update.Message.IsGroup() &&
                   _inMemorySettingsService.GetSettingValue(InMemorySettingKey.WelcomeMessageSending, false) &&
                   update.Message.NewChatMembers != null;
        }

        protected override async Task<IUpdateHandlerResult> HandleUpdateInternal(Update update)
        {
            LogNewUsers(update.Message.NewChatMembers, update.Message.Chat);
            
            var savedChat = await _savedChatRepository.GetByName(update.Message.Chat.Title);

            if (savedChat == null)
            {
                Logger.LogInformation("Skipped welcome message!");
                return Result.Nothing();
            }

            var welcomeMessage = JsonConvert.DeserializeObject<WelcomeMessage>(savedChat.WelcomeMessage);

            if (!welcomeMessage.IsOn || welcomeMessage.Message.IsBlank())
            {
                Logger.LogInformation("Skipped welcome message!");
                return Result.Nothing();
            }

            var mentions = string.Join(", ",
                update.Message.NewChatMembers
                    .Select(u => u.GetMentionHtmlLink())
                    .ToArray());

            var message = $"{mentions}\n\n{welcomeMessage.Message}";

            var markup = InlineKeyboardHelper.GetWelcomeButton(welcomeMessage);
            
            return Result.Text(update.Message.Chat.Id, message, update.Message.MessageId, ParseMode.Html, false, markup);
        }

        private void LogNewUsers(User[] users, Chat chat)
        {
            if (users.Length == 0)
            {
                Logger.LogInformation("Got empty 'new user' update in chat [{ChatId} | {ChatTitle}]", chat.Id, chat.Title);
            }
            
            foreach (var user in users) 
            {
                Logger.LogInformation("New users [{UserId} | {UserLogin} | {UserFirstName}] joined to chat [{ChatId} | {ChatTitle}]", 
                    user.Id, user.Username, user.FirstName, chat.Id, chat.Title);
            }
        }
    }
}
