using System.Threading.Tasks;
using CommunityBot.Contracts;
using CommunityBot.Handlers.Results;
using CommunityBot.Helpers;
using CommunityBot.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace CommunityBot.Handlers.BotCommands.User
{
    public class CollectUserInfoCommand : BotCommandHandlerBase
    {
        private readonly InMemorySettingsService _inMemorySettingsService;

        public CollectUserInfoCommand(
            IOptions<BotConfigurationOptions> options, 
            ILoggerFactory logger,
            InMemorySettingsService inMemorySettingsService)
            : base(options, logger)
        {
            _inMemorySettingsService = inMemorySettingsService;
        }

        protected override BotCommandConfig Config { get; } = new("collect_user_info", isForAdmin: true, allowOnlyInPrivate: true);
        protected override Task<IUpdateHandlerResult> HandleUpdateInternal(Update update, string value)
        {
            if (value.IsBlank() || value.NotIn("on", "off"))
            {
                return Result.Text(update.Message.Chat.Id,
                    "Пожалуйста, добавьте после комманды 'on' или 'off' для понимания: включить или выключить.",
                    update.Message.MessageId).AsTask();
            }

            _inMemorySettingsService.SetSettingValue(InMemorySettingKey.CollectUserInfo, value == "on");
            
            return Result.Text(update.Message.Chat.Id,
                "Значение обновлено!",
                update.Message.MessageId).AsTask();
        }
    }
}
