using System.Threading.Tasks;
using CommunityBot.Contracts;
using CommunityBot.Handlers.Results;
using CommunityBot.Helpers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;

namespace CommunityBot.Handlers.BotCommands.ChatManage
{
    public class AddThisChatBotCommand : BotCommandHandlerBase
    {
        private readonly ITelegramBotClient _botClient;
        private readonly IChatRepository _chatRepository;

        public AddThisChatBotCommand(
            ITelegramBotClient botClient, 
            IOptions<BotConfigurationOptions> options,
            IChatRepository chatRepository,
            ILoggerFactory logger)
            : base(options, logger)
        {
            _botClient = botClient;
            _chatRepository = chatRepository;
        }

        protected override BotCommandConfig Config { get; } = new ("add_this_chat");
        protected override async Task<IUpdateHandlerResult> HandleUpdateInternal(Update update, string inviteLink)
        {
            var chat = update.Message.Chat;
            
            if (chat.IsPrivate())
            {
                return Result.Text(chat.Id, "Зачем ты пытаешься добавить наш личный чат в список чатов? >_>", update.Message.MessageId);
            }

            if (!chat.IsGroup())
            {
                return Result.Text(chat.Id, "Добавить можно только группу!", update.Message.MessageId);
            }

            if (inviteLink.IsNotBlank())
            {
                if (!inviteLink.StartsWith("https://t.me/joinchat/"))
                {
                    return Result.Text(chat.Id,
                        "Неправильная ссылка приглашение: ссылка должна начинаться с 'https://t.me/joinchat/'. Добавлять ссылку на публичные чаты не нужно.",
                        update.Message.MessageId);
                }
                
                chat.InviteLink = inviteLink.Trim();
            }

            if (chat.InviteLink.IsBlank())
            {
                try
                {
                    chat.InviteLink = await _botClient.ExportChatInviteLinkAsync(chat.Id);
                }
                catch (ApiRequestException e)
                {
                    Logger.LogWarning("Can't get invite link for chat {chatId}! [ExMessage: {exMessage}, StackTrace: {stackTrace}]", chat.Id, e.Message, e.StackTrace);
                }

                if (chat.InviteLink.IsBlank())
                {
                    return Result.Text(chat.Id,
                        "Или дайте мне ссылку-приглашение вместе с коммандой, или сделайте админом, чтобы я сам мог создать её.",
                        update.Message.MessageId);
                }
            }

            await _chatRepository.AddOrUpdate(new SavedChat(chat.Id, chat.Title, chat.InviteLink));

            return Result.Text(chat.Id, "Чат добавлен! Спасибо за помощь боту!", update.Message.MessageId);
        }
    }
}
