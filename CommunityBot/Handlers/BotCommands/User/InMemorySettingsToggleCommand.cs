using System;
using System.Threading.Tasks;
using CommunityBot.Contracts;
using CommunityBot.Handlers.Results;
using CommunityBot.Helpers;
using CommunityBot.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Telegram.Bot.Types;

namespace CommunityBot.Handlers.BotCommands.User
{
    public class InMemorySettingsToggleCommand : BotCommandHandlerBase
    {
        private readonly InMemorySettingsService _inMemorySettingsService;

        public InMemorySettingsToggleCommand(
            IOptions<BotConfigurationOptions> options, 
            ILoggerFactory logger,
            InMemorySettingsService inMemorySettingsService)
            : base(options, logger)
        {
            _inMemorySettingsService = inMemorySettingsService;
        }

        protected override BotCommandConfig Config { get; } = new("set_setting", isForAdmin: true, allowOnlyInPrivate: true);
        protected override Task<IUpdateHandlerResult> HandleUpdateInternal(Update update, string raw)
        {
            var split = raw.Split(" ");

            if (split.Length != 2)
            {
                return ReplyPlainText(update, "Введите: '/set_setting код_настройки on/off'").AsTask();
            }
            
            var settingKeyRaw = split[0].Trim();
            var value = split[1].Trim();

            if (!Enum.TryParse(settingKeyRaw, true, out InMemorySettingKey settingKey))
            {
                return ReplyPlainText(update, $"Команда {settingKeyRaw} не найдена!").AsTask();
            }
            
            if (value.NotIn("on", "off"))
            {
                return Result.Text(update.Message.Chat.Id,
                    "Пожалуйста, добавьте после комманды 'on' или 'off' для понимания: включить или выключить.",
                    update.Message.MessageId).AsTask();
            }

            _inMemorySettingsService.SetSettingValue(settingKey, value == "on");
            
            return Result.Text(update.Message.Chat.Id,
                "Значение обновлено!",
                update.Message.MessageId).AsTask();
        }
    }
}
