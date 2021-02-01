using System.Threading.Tasks;
using CommunityBot.Contracts;
using CommunityBot.Helpers;
using CommunityBot.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace CommunityBot.Handlers
{
    public class UserDataCollectorUpdateHandler : UpdateHandlerBase
    {
        private readonly IAppUserRepository _appUserRepository;
        private readonly InMemorySettingsService _inMemorySettingsService;

        public UserDataCollectorUpdateHandler(
            IAppUserRepository appUserRepository,
            ITelegramBotClient botClient, 
            IOptions<BotConfigurationOptions> options,
            InMemorySettingsService inMemorySettingsService,
            ILogger<UserDataCollectorUpdateHandler> logger)
            : base(botClient, options, logger)
        {
            _appUserRepository = appUserRepository;
            _inMemorySettingsService = inMemorySettingsService;
        }

        protected override UpdateType[] AllowedUpdates => new[] {UpdateType.Message};

        public override int OrderNumber => 99;

        protected override bool CanHandle(Update update)
        {
            return _inMemorySettingsService.GetSettingCollectUserInfo() && update.Message.From != null;
        }

        protected override async Task HandleUpdateInternalAsync(Update update)
        {
            var isExisted = await _appUserRepository.IsExisted(update.Message.From.Id);

            if (!isExisted)
            {
                await _appUserRepository.Add(update.Message.From.ToAppUser());
            }
        }
    }
}