using System;
using System.Threading.Tasks;
using CommunityBot.Contracts;
using CommunityBot.Handlers.Results;
using CommunityBot.Helpers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace CommunityBot.Handlers.BotCommands.User
{
    public class AddUsersFromJsonCommand : BotCommandHandlerBase
    {
        private readonly ITelegramBotClient _botClient;
        private readonly IAppUserRepository _appUserRepository;

        public AddUsersFromJsonCommand(
            ITelegramBotClient botClient, 
            IOptions<BotConfigurationOptions> options, 
            ILoggerFactory logger,
            IAppUserRepository appUserRepository)
            : base(options, logger)
        {
            _botClient = botClient;
            _appUserRepository = appUserRepository;
        }

        protected override BotCommandConfig Config { get; } =
            new("add_users_from_json", isForAdmin: true, allowOnlyInPrivate: true);
        protected override async Task<IUpdateHandlerResult> HandleUpdateInternal(Update update, string commandArg)
        {
            if (update.Message.Type != MessageType.Document)
            {
                return Result.Text(update.Message.Chat.Id,
                    "Вместе с коммандой необходимо приложить json файл с массивом юзеров внутри.",
                    update.Message.MessageId);
            }

            var json = await _botClient.DownloadStringFile(update.Message.Document.FileId);

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
