using System;
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
    public class ChatManageUpdateHandler : UpdateHandlerBase
    {
        private readonly IChatRepository _chatRepository;
        private const string AddChatCommand = "add_chat";
        private const string AddThisChatCommand = "add_this_chat";
        private const string RemoveChatCommand = "remove_chat";
        
        public ChatManageUpdateHandler(
            ITelegramBotClient botClient, 
            IOptions<BotConfigurationOptions> options,
            IChatRepository chatRepository,
            ILogger<UpdateHandlerBase> logger) 
            : base(botClient, options, logger)
        {
            _chatRepository = chatRepository;
        }

        protected override UpdateType[] AllowedUpdates => new[] {UpdateType.Message};

        protected override bool CanHandle(Update update)
        {
            return new[] {AddChatCommand, AddThisChatCommand, RemoveChatCommand}
                .Contains(update.Message.GetFirstBotCommand()?.name);
        }

        protected override async Task HandleUpdateInternalAsync(Update update)
        {
            var command = update.Message.GetFirstBotCommand()!.Value;

            if (command.name == AddChatCommand)
            {
                await AddChat(command.arg, update.Message.MessageId);
                return;
            }

            if (command.name == AddThisChatCommand)
            {
                await AddThisChat(command.arg, update.Message.Chat, update.Message.MessageId);
                return;
            }

            if (command.name == RemoveChatCommand)
            {
                await RemoveChat(command.arg, update.Message.From.Username, update.Message.MessageId);
            }
        }

        private async Task SendMessage(string text, int replyToMessageId)
        {
            await BotClient.SendTextMessageAsync(Options.MainChannelId, text, replyToMessageId: replyToMessageId);
        }

        private async Task AddChat(string chatRawArgs, int replyToMessageId)
        {
            var arg = chatRawArgs.Split('\n', StringSplitOptions.RemoveEmptyEntries);

            if (arg.Length < 2)
            {
                await SendMessage("Неправильно отправленная команда. Пожалуйста попробуйте ещё раз или обратитесь к админам за помощью.", replyToMessageId);
            }

            if (!arg[1].StartsWith("https://t.me/joinchat/"))
            {
                await SendMessage(
                    "Неправильная ссылка приглашение: ссылка должна начинаться с 'https://t.me/joinchat/'. Добавлять ссылку на публичные чаты не нужно.",
                    replyToMessageId);
            }
                
            var chatName = arg[0];
            var chatLink = arg[1];

            await _chatRepository.AddOrUpdate(new SavedChat(-1, chatName, chatLink));

            await SendMessage("Чат добавлен/обновлён! Спасибо за помощь боту!", replyToMessageId);
        }

        private async Task AddThisChat(string inviteLink, Chat chat, int replyToMessageId)
        {
            if (chat.IsPrivate())
            {
                await SendMessage("Зачем ты пытаешься добавить наш личный чат в список чатов? >_>", replyToMessageId);
                return;
            }

            if (!chat.IsGroup())
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(inviteLink) && string.IsNullOrWhiteSpace(chat.InviteLink))
            {
                chat.InviteLink = await BotClient.ExportChatInviteLinkAsync(chat.Id);

                if (string.IsNullOrWhiteSpace(chat.InviteLink))
                {
                    await SendMessage("Или дайте мне ссылку-приглашение вместе с коммандой, или сделайте админом, чтобы я сам мог создать её.", replyToMessageId);
                    return;
                }
            }

            await _chatRepository.AddOrUpdate(new SavedChat(chat.Id, chat.Title, chat.InviteLink));
                
            await SendMessage("Чат добавлен! Спасибо за помощь боту!", replyToMessageId);
        }

        private async Task RemoveChat(string chatExactName, string fromUserName, int replyToMessageId)
        {
            if (!Options.Admins.Contains(fromUserName))
            {
                await SendMessage("Если хочещь удалить чат из моего списка, то попроси админов.", replyToMessageId);
                return;
            }

            if (string.IsNullOrWhiteSpace(chatExactName))
            {
                await SendMessage("Напиши рядом с командой полное имя чата, который удаляем.", replyToMessageId);
                return;
            }

            await _chatRepository.Remove(chatExactName);

            await SendMessage($"Если чат с названием {chatExactName} существовал в моём списке, то я его удалил.", replyToMessageId);
        }
    }
}