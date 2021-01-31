using System.Linq;
using System.Threading.Tasks;
using CommunityBot.Contracts;
using CommunityBot.Helpers;
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
        private const string SetPasswordCommand = "set_password";

        public UserUpdateHandler(
            ITelegramBotClient botClient,
            IOptions<BotConfigurationOptions> options,
            IAppUserRepository appUserRepository,
            ILogger<UserUpdateHandler> logger)
            : base(botClient, options, logger)
        {
            _appUserRepository = appUserRepository;
        }

        protected override UpdateType[] AllowedUpdates => new[] {UpdateType.Message};
        protected override bool CanHandle(Update update)
        {
            return update.Message.IsPrivate() &&
                   new[] {SetPasswordCommand}.Contains(update.Message.GetFirstBotCommand()?.name);
        }

        protected override async Task HandleUpdateInternalAsync(Update update)
        {
            var command = update.Message.GetFirstBotCommand()!.Value;

            if (command.name != SetPasswordCommand)
            {
                return;
            }
            
            if (command.arg.IsBlank())
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
                appUser.PasswordHash = StringExtensions.CreateMd5(command.arg.Trim());
                await _appUserRepository.Add(appUser);
            }
            appUser.PasswordHash = StringExtensions.CreateMd5(command.arg);
            await _appUserRepository.Update(appUser);
            
            await BotClient.SendTextMessageAsync(update.Message.Chat.Id,
                $"Пароль обновлён! Ваш новый пароль: '{command.arg}' (без кавычек). Из пароля были удалены пробелы в начале и в конце, если они были.",
                replyToMessageId: update.Message.MessageId);
        }
    }
}