using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

using CommunityBot.Contracts;
using CommunityBot.Helpers;

namespace CommunityBot.Handlers
{
    public class ChatManageUpdateHandler : UpdateHandlerBase
    {
        private readonly IChatRepository _chatRepository;
        private const string AddChatCommand = "add_chat";
        private const string AddThisChatCommand = "add_this_chat";
        private const string RemoveChatCommand = "remove_chat";
        private const string GetAllChatsCommand = "get_all_chats";
        private const string GetIdOfThisChat = "get_id_of_this_chat";
        
        public ChatManageUpdateHandler(
            ITelegramBotClient botClient, 
            IOptions<BotConfigurationOptions> options,
            IChatRepository chatRepository,
            ILogger<ChatManageUpdateHandler> logger) 
            : base(botClient, options, logger)
        {
            _chatRepository = chatRepository;
        }

        protected override UpdateType[] AllowedUpdates => new[] {UpdateType.Message};

        protected override bool CanHandle(Update update)
        {
            return new[] {AddChatCommand, AddThisChatCommand, RemoveChatCommand, GetAllChatsCommand, GetIdOfThisChat}
                .Contains(update.Message.GetFirstBotCommand()?.name);
        }

        protected override async Task HandleUpdateInternalAsync(Update update)
        {
            var command = update.Message.GetFirstBotCommand()!.Value;

            switch (command.name)
            {
                case AddChatCommand:
                    await AddChat(command.arg, update);
                    return;
                case AddThisChatCommand:
                    await AddThisChat(command.arg, update);
                    return;
                case RemoveChatCommand:
                    await RemoveChat(command.arg, update);
                    return;
                case GetAllChatsCommand:
                    await GetAllChats(update);
                    return;
                case GetIdOfThisChat:
                    await SendMessage(update.Message.Chat.Id, $"ID этого чата: {update.Message.Chat.Id}", update.Message.MessageId);
                    return;
            }
        }

        private async Task SendMessage(long replyChatId, string text, int replyToMessageId, ParseMode parseMode = ParseMode.Default)
        {
            await BotClient.SendTextMessageAsync(replyChatId, text, replyToMessageId: replyToMessageId, disableWebPagePreview: true, parseMode: parseMode);
        }

        private async Task AddChat(string chatRawArgs, Update update)
        {
            var arg = chatRawArgs.Split('\n', StringSplitOptions.RemoveEmptyEntries);

            if (arg.Length < 2)
            {
                await SendMessage(update.Message.Chat.Id, "Неправильно отправленная команда. Пожалуйста попробуйте ещё раз или обратитесь к админам за помощью.", update.Message.MessageId);
                return;
            }

            if (!arg[1].StartsWith("https://t.me/joinchat/"))
            {
                await SendMessage(
                    update.Message.Chat.Id,
                    "Неправильная ссылка приглашение: ссылка должна начинаться с 'https://t.me/joinchat/'. Добавлять ссылку на публичные чаты не нужно.",
                    update.Message.MessageId);
                return;
            }
                
            var chatName = arg[0];
            var chatLink = arg[1];

            await _chatRepository.AddOrUpdate(new SavedChat(-1, chatName, chatLink));

            await SendMessage(update.Message.Chat.Id, "Чат добавлен/обновлён! Спасибо за помощь боту!", update.Message.MessageId);
        }

        private async Task AddThisChat(string inviteLink, Update update)
        {
            var chat = update.Message.Chat;
            
            if (chat.IsPrivate())
            {
                await SendMessage(chat.Id, "Зачем ты пытаешься добавить наш личный чат в список чатов? >_>", update.Message.MessageId);
                return;
            }

            if (!chat.IsGroup())
            {
                return;
            }

            if (inviteLink.IsBlank() && chat.InviteLink.IsBlank())
            {
                try
                {
                    chat.InviteLink = await BotClient.ExportChatInviteLinkAsync(chat.Id);
                }
                catch (ApiRequestException e)
                {
                    Logger.LogWarning("Can't get invite link for chat {chatId}! [ExMessage: {exMessage}, StackTrace: {stackTrace}]", chat.Id, e.Message, e.StackTrace);
                }

                if (chat.InviteLink.IsBlank())
                {
                    await SendMessage(chat.Id, "Или дайте мне ссылку-приглашение вместе с коммандой, или сделайте админом, чтобы я сам мог создать её.", update.Message.MessageId);
                    return;
                }
            }

            await _chatRepository.AddOrUpdate(new SavedChat(chat.Id, chat.Title, chat.InviteLink));
                
            await SendMessage(chat.Id, "Чат добавлен! Спасибо за помощь боту!", update.Message.MessageId);
        }

        private async Task RemoveChat(string chatExactName, Update update)
        {
            if (!Options.Admins.Contains(update.Message.From.Username))
            {
                await SendMessage(update.Message.Chat.Id, "Если хочешь удалить чат из моего списка, то попроси админов.", update.Message.MessageId);
                return;
            }

            if (chatExactName.IsBlank())
            {
                await SendMessage(update.Message.Chat.Id, "Напиши рядом с командой полное имя чата, который удаляем.", update.Message.MessageId);
                return;
            }

            await _chatRepository.RemoveByName(chatExactName);

            await SendMessage(update.Message.Chat.Id, $"Если чат с названием {chatExactName} существовал в моём списке, то я его удалил.", update.Message.MessageId);
        }

        private async Task GetAllChats(Update update)
        {
            if (!IsFromAdmin(update) || !update.Message.IsPrivate())
            {
                await SendMessage(update.Message.Chat.Id, "Смотреть список всех чатов можно только админам и только в ЛС.", update.Message.MessageId);
                return;
            }

            var chats = await _chatRepository.GetAll();
            
            var chatMarkup = chats.Select(c =>
                    $"{c.JoinLink.ToHtmlLink(c.ExactName)}\n{c.JoinLink.ToMonospace()}")
                .ToArray();

            var resultMessage = chatMarkup.Any()
                ? string.Join("\n\n", chatMarkup) 
                : "Список чатов пуст!\nЕго можно пополнить при помощи команды /add_chat";

            await SendMessage(update.Message.Chat.Id, resultMessage, update.Message.MessageId, ParseMode.Html);
        }
    }
}