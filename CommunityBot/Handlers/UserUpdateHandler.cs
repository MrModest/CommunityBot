using System.Linq;
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
    public class UserUpdateHandler : UpdateHandlerBase
    {
        private readonly IAppUserRepository _appUserRepository;
        private readonly InMemorySettingsService _inMemorySettingsService;
        private const string SetPasswordCommand = "set_password";
        private const string CollectUserInfoCommand = "collect_user_info";

        public UserUpdateHandler(
            ITelegramBotClient botClient,
            IOptions<BotConfigurationOptions> options,
            IAppUserRepository appUserRepository,
            InMemorySettingsService inMemorySettingsService,
            ILogger<UserUpdateHandler> logger)
            : base(botClient, options, logger)
        {
            _appUserRepository = appUserRepository;
            _inMemorySettingsService = inMemorySettingsService;
        }

        protected override UpdateType[] AllowedUpdates => new[] {UpdateType.Message};
        protected override bool CanHandle(Update update)
        {
            return update.Message.IsPrivate() &&
                   new[] {SetPasswordCommand, CollectUserInfoCommand}.Contains(update.Message.GetFirstBotCommand()?.name);
        }

        protected override async Task HandleUpdateInternalAsync(Update update)
        {
            var command = update.Message.GetFirstBotCommand()!.Value;

            switch (command.name)
            {
                case SetPasswordCommand:
                    await SetPassword(update, command.arg);
                    break;
                case CollectUserInfoCommand:
                    await SetCollectUserInfoSetting(update, command.arg);
                    break;
            }
        }

        private async Task SetPassword(Update update, string password)
        {
            if (password.IsBlank())
            {
                await BotClient.SendTextMessageAsync(update.Message.Chat.Id,
                    "Пожалуйста, введите свой пароль сразу после команды! Например '/set_password 123'",
                    replyToMessageId: update.Message.MessageId);
                return;
            }

            var appUser = await _appUserRepository.Get(update.Message.From.Id);
            if (appUser == null)
            {
                appUser = update.Message.From.ToAppUser();
                appUser.PasswordHash = StringExtensions.CreateMd5(password.Trim());
                await _appUserRepository.Add(appUser);
            }
            appUser.PasswordHash = StringExtensions.CreateMd5(password);
            await _appUserRepository.Update(appUser);
            
            await BotClient.SendTextMessageAsync(update.Message.Chat.Id,
                $"Пароль обновлён! Ваш новый пароль: '{password}' (без кавычек). Из пароля были удалены пробелы в начале и в конце, если они были.",
                replyToMessageId: update.Message.MessageId);
        }

        private async Task SetCollectUserInfoSetting(Update update, string value)
        {
            if (!IsFromAdmin(update))
            {
                await BotClient.SendTextMessageAsync(update.Message.Chat.Id,
                    "Данная команда доступна только администраторам!",
                    replyToMessageId: update.Message.MessageId);
                return;
            }
                
            if (value.IsBlank() || value.NotIn("on", "off"))
            {
                await BotClient.SendTextMessageAsync(update.Message.Chat.Id,
                    "Пожалуйста, добавьте после комманды 'on' или 'off' для понимания: включить или выключить.",
                    replyToMessageId: update.Message.MessageId);
                return;
            }

            _inMemorySettingsService.SetSettingCollectUserInfo(value == "on");
            
            await BotClient.SendTextMessageAsync(update.Message.Chat.Id,
                "Значение обновлено!",
                replyToMessageId: update.Message.MessageId);
        }
    }
}