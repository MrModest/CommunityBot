using System.Threading.Tasks;
using CommunityBot.Contracts;
using CommunityBot.Handlers.Results;
using CommunityBot.Helpers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace CommunityBot.Handlers.BotCommands.User
{
    public class SetPasswordCommand : BotCommandHandlerBase
    {
        private readonly IAppUserRepository _appUserRepository;

        public SetPasswordCommand(
            ITelegramBotClient botClient, 
            IOptions<BotConfigurationOptions> options, 
            ILoggerFactory logger,
            IAppUserRepository appUserRepository)
            : base(botClient, options, logger)
        {
            _appUserRepository = appUserRepository;
        }

        protected override BotCommandConfig Config { get; } =
            new("set_password", allowOnlyInPrivate: true, argRequiredMessage: "Пожалуйста, введите свой пароль сразу после команды! Например '/set_password 123'");
        protected override async Task<IUpdateHandlerResult> HandleUpdateInternal(Update update, string password)
        {
            var appUser = await _appUserRepository.Get(update.Message.From.Id);
            if (appUser == null)
            {
                appUser = update.Message.From.ToAppUser();
                appUser.PasswordHash = StringExtensions.CreateMd5(password.Trim());
                await _appUserRepository.Add(appUser);
            }
            appUser.PasswordHash = StringExtensions.CreateMd5(password);
            await _appUserRepository.Update(appUser);
            
            return Result.Text(update.Message.Chat.Id,
                $"Пароль обновлён! Ваш новый пароль: <code>{password}</code>\n" +
                "Из пароля были удалены пробелы в начале и в конце, если они были.",
                update.Message.MessageId, ParseMode.Html);
        }
    }
}