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
using CommunityBot.Handlers.Results;
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
            return update.Message.ContainCommand(AddChatCommand, AddThisChatCommand, RemoveChatCommand, GetAllChatsCommand, GetIdOfThisChat);
        }

        protected override async Task<IUpdateHandlerResult> HandleUpdateInternalAsync(Update update)
        {
            var command = update.Message.GetFirstBotCommand()!.Value;

            switch (command.name)
            {
                case AddChatCommand:
                    return await AddChat(command.arg, update);
                
                case AddThisChatCommand:
                    return await AddThisChat(command.arg, update);
                
                case RemoveChatCommand:
                    return await RemoveChat(command.arg, update);
                
                case GetAllChatsCommand:
                    return await GetAllChats(update);
                
                case GetIdOfThisChat:
                    return Result.Text(update.Message.Chat.Id, $"ID этого чата: {update.Message.Chat.Id}", update.Message.MessageId);
                
                default:
                    return Result.Nothing();
            };
        }

        private async Task<IUpdateHandlerResult> AddChat(string chatRawArgs, Update update)
        {
            var arg = chatRawArgs.Split('\n', StringSplitOptions.RemoveEmptyEntries);

            if (arg.Length < 2)
            {
                return Result.Text(update.Message.Chat.Id, "Неправильно отправленная команда. Пожалуйста попробуйте ещё раз или обратитесь к админам за помощью.", update.Message.MessageId);
            }

            if (!arg[1].StartsWith("https://t.me/joinchat/"))
            {
                return Result.Text(
                    update.Message.Chat.Id,
                    "Неправильная ссылка приглашение: ссылка должна начинаться с 'https://t.me/joinchat/'. Добавлять ссылку на публичные чаты не нужно.",
                    update.Message.MessageId);
            }
                
            var chatName = arg[0];
            var chatLink = arg[1];

            await _chatRepository.AddOrUpdate(new SavedChat(-1, chatName, chatLink));

            return Result.Text(update.Message.Chat.Id, "Чат добавлен/обновлён! Спасибо за помощь боту!", update.Message.MessageId);
        }

        private async Task<IUpdateHandlerResult> AddThisChat(string inviteLink, Update update)
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
                    chat.InviteLink = await BotClient.ExportChatInviteLinkAsync(chat.Id);
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

        private async Task<IUpdateHandlerResult> RemoveChat(string chatExactName, Update update)
        {
            if (!Options.Admins.Contains(update.Message.From.Username))
            {
                return Result.Text(update.Message.Chat.Id, "Если хочешь удалить чат из моего списка, то попроси админов.", update.Message.MessageId);
            }

            if (chatExactName.IsBlank())
            {
                return Result.Text(update.Message.Chat.Id, "Напиши рядом с командой полное имя чата, который удаляем.", update.Message.MessageId);
            }

            await _chatRepository.RemoveByName(chatExactName);

            return Result.Text(update.Message.Chat.Id, $"Если чат с названием {chatExactName} существовал в моём списке, то я его удалил.", update.Message.MessageId);
        }

        private async Task<IUpdateHandlerResult> GetAllChats(Update update)
        {
            if (!IsFromAdmin(update) || !update.Message.IsPrivate())
            {
                return Result.Text(update.Message.Chat.Id, "Смотреть список всех чатов можно только админам и только в ЛС.", update.Message.MessageId);
            }

            var chats = await _chatRepository.GetAll();
            
            var chatMarkup = chats.Select(c =>
                    $"{c.JoinLink.ToHtmlLink(c.ExactName)}\n{c.JoinLink.ToMonospace()}")
                .ToArray();

            var resultMessage = chatMarkup.Any()
                ? string.Join("\n\n", chatMarkup) 
                : "Список чатов пуст!\nЕго можно пополнить при помощи команды /add_chat";

            return Result.Text(update.Message.Chat.Id, resultMessage, update.Message.MessageId, ParseMode.Html);
        }
    }
}