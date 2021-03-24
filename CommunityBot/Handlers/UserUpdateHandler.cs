using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CommunityBot.Contracts;
using CommunityBot.Handlers.Results;
using CommunityBot.Helpers;
using CommunityBot.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
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
        private const string AddUsersFromJsonCommand = "add_users_from_json";

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
                   update.Message.ContainCommand(SetPasswordCommand, CollectUserInfoCommand, AddUsersFromJsonCommand);
        }

        protected override async Task<IUpdateHandlerResult> HandleUpdateInternal(Update update)
        {
            var command = update.Message.GetFirstBotCommand()!.Value;

            switch (command.name)
            {
                case SetPasswordCommand:
                    return await SetPassword(update, command.arg);
                case CollectUserInfoCommand:
                    return await SetCollectUserInfoSetting(update, command.arg);
                case AddUsersFromJsonCommand:
                    return await AddUsersFromJson(update);
                
                default:
                    return Result.Nothing();
            }
        }

        private async Task<IUpdateHandlerResult> SetPassword(Update update, string password)
        {
            if (password.IsBlank())
            {
                return Result.Text(update.Message.Chat.Id,
                    "Пожалуйста, введите свой пароль сразу после команды! Например '/set_password 123'",
                    update.Message.MessageId);
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
            
            return Result.Text(update.Message.Chat.Id,
                $"Пароль обновлён! Ваш новый пароль: <code>{password}</code>\n" +
                "Из пароля были удалены пробелы в начале и в конце, если они были.",
                update.Message.MessageId, ParseMode.Html);
        }

        private Task<IUpdateHandlerResult> SetCollectUserInfoSetting(Update update, string value)
        {
            if (!IsFromAdmin(update))
            {
                return Result.Text(update.Message.Chat.Id,
                    "Данная команда доступна только администраторам!",
                    update.Message.MessageId).AsTask();
            }
                
            if (value.IsBlank() || value.NotIn("on", "off"))
            {
                return Result.Text(update.Message.Chat.Id,
                    "Пожалуйста, добавьте после комманды 'on' или 'off' для понимания: включить или выключить.",
                    update.Message.MessageId).AsTask();
            }

            _inMemorySettingsService.SetSettingCollectUserInfo(value == "on");
            
            return Result.Text(update.Message.Chat.Id,
                "Значение обновлено!",
                update.Message.MessageId).AsTask();
        }

        private async Task<IUpdateHandlerResult> AddUsersFromJson(Update update)
        {
            if (!IsFromAdmin(update))
            {
                return Result.Text(update.Message.Chat.Id,
                    "Данная команда доступна только администраторам!",
                    update.Message.MessageId);
            }
            
            if (update.Message.Type != MessageType.Document)
            {
                return Result.Text(update.Message.Chat.Id,
                    "Вместе с коммандой необходимо приложить json файл с массивом юзеров внутри.",
                    update.Message.MessageId);
            }
            
            await using var stream = new MemoryStream();
            await BotClient.GetInfoAndDownloadFileAsync(update.Message.Document.FileId, stream);

            if (stream.Position != 0) 
            {
                if (!stream.CanSeek)
                {
                    throw new InvalidOperationException(
                        $"Can't seek file '{update.Message.Document.FileId}' | '{update.Message.Document.FileName}' | '{update.Message.Document.FileSize}'");
                }
                stream.Position = 0;
            }

            var json = await new StreamReader(stream).ReadToEndAsync();

            try
            {
                var users = JsonConvert.DeserializeObject<AppUser[]>(json);

                foreach (var user in users)
                {
                    if (await _appUserRepository.IsExisted(user.Id))
                    {
                        await _appUserRepository.Update(user);
                    }
                    else
                    {
                        await _appUserRepository.Add(user);
                    }
                }
                Logger.LogWarning("Следующие пользователи были добавлены или обновлены ({UserCount})\n\n: {Users}", users.Length, string.Join<AppUser>("\n", users));

                return Result.Text(update.Message.Chat.Id, 
                    $"Следующие пользователи были добавлены или обновлены ({users.Length})\n\n: {string.Join<AppUser>("\n", users)}",
                    update.Message.MessageId);
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Не удалось десериализовать файл: {ExMessage} | {ExStackTrace}\n\n{Json}", e.Message, e.StackTrace, json);
                return Result.Text(update.Message.Chat.Id,
                    $"Не удалось десериализовать файл: {e.Message} | {e.StackTrace}",
                    update.Message.MessageId);
            }
        }
    }
}