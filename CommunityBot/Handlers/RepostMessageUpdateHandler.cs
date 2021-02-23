using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

using CommunityBot.Contracts;
using CommunityBot.Helpers;
using Microsoft.Extensions.Logging;
using Chat = Telegram.Bot.Types.Chat;

namespace CommunityBot.Handlers
{
    public class RepostMessageUpdateHandler : UpdateHandlerBase
    {
        private const string CreatePostCommand = "event";
        
        private readonly IChatRepository _chatRepository;
        private readonly IMediaGroupService _mediaGroupService;

        public RepostMessageUpdateHandler(
            ITelegramBotClient botClient,
            IOptions<BotConfigurationOptions> options,
            ILogger<RepostMessageUpdateHandler> logger,
            IChatRepository chatRepository,
            IMediaGroupService mediaGroupService) 
            : base(botClient, options, logger)
        {
            _chatRepository = chatRepository;
            _mediaGroupService = mediaGroupService;
        }
        
        protected override UpdateType[] AllowedUpdates => new[] {UpdateType.Message};

        protected override bool CanHandle(Update update)
        {
            var command = update.Message.GetFirstBotCommand();
            
            return command.HasValue && command.Value.name == CreatePostCommand && command.Value.arg.IsNotBlank() ||
                   update.Message.HasMentionOfUserName(Options.BotName);
        }

        protected override async Task HandleUpdateInternalAsync(Update update)
        {
            Message? message = null;
            
            if (update.Message.GetFirstBotCommand()?.name == CreatePostCommand)
            {
                message = update.Message;
            }

            if (update.Message.HasMentionOfUserName(Options.BotName))
            {
                message = update.Message.ReplyToMessage;
            }

            if (message != null)
            {
                await SendPost(message);
                return;
            }
            
            Logger.LogInformation($"Update {update.ToLog()} was skipped!");
        }

        private async Task SendPost(Message message)
        {
            switch (message.Type)
            {
                case MessageType.Text:
                    await SendTextPost(message);
                    break;
                case MessageType.Photo:
                case MessageType.Video:
                    if (message.MediaGroupId.IsBlank())
                    {
                        await SendPhotoVideoPost(message);    
                    }
                    else
                    {
                        await SendMediaGroupPost(message);
                    }
                    break;
            }
        }

        private async Task SendTextPost(Message message)
        {
            var text = await PreparePost(message);
            
            await BotClient.SendTextMessageAsync(Options.MainChannelId, text, ParseMode.Html,
                disableWebPagePreview: true);
        }

        private async Task SendPhotoVideoPost(Message message)
        {
            var caption = await PreparePost(message);

            switch (message.Type)
            {
                case MessageType.Photo:
                {
                    var photo = message.Photo.GetLargestPhotoSize().FileId;
                
                    await BotClient.SendPhotoAsync(Options.MainChannelId, photo, caption, ParseMode.Html);
                    break;
                }
                case MessageType.Video:
                    await BotClient.SendVideoAsync(Options.MainChannelId, message.Video.FileId, caption: caption, parseMode: ParseMode.Html);
                    break;
                
                default:
                    throw new InvalidOperationException($"Not supported MessageType: {message.Type}");
            }
        }

        private async Task SendMediaGroupPost(Message message)
        {
            var media = _mediaGroupService.GetMediaByGroupId(message.MediaGroupId);

            if (media == null)
            {
                Logger.LogWarning("Post was not send because not found media group with id '{id}'", message.MediaGroupId);
                return;
            }

            foreach (var inputMedia in media.Where(m => m.Caption != null).OfType<InputMediaBase>())
            {
                message.Text = inputMedia.Caption;
                inputMedia.Caption = await PreparePost(message);
            }
            
            await BotClient.SendMediaGroupAsync(media, Options.MainChannelId);
        }

        private async Task<string> PreparePost(Message message)
        {
            var post = new StringBuilder();
            var postText = message.GetFirstBotCommand()?.arg ?? message.Text ?? message.Caption;

            postText = MessageEntityWrapper.GetMarkupMessage(
                postText,
                message.Text != null 
                    ? message.Entities 
                    : message.CaptionEntities, 
                ParseMode.Html);
            
            post.Append($"{postText}\n\n");
            post.Append($" — {message.From.GetMentionHtmlLink()}");
            
            if (!message.IsPrivate())
            {
                post.Append($" из {await GetChatLink(message.Chat)}\n");
                post.Append($" — {message.GetPostLink().ToHtmlLink("Источник")}");
            }
            
            return post.ToString();
        }

        private async Task<string> GetChatLink(Chat chat)
        {
            if (chat.Username.IsNotBlank())
            {
                return $"https://t.me/{chat.Username}".ToHtmlLink(chat.Title);
            }
            
            var savedChat = await _chatRepository.GetByName(chat.Title);

            return savedChat != null
                ? savedChat.JoinLink.ToHtmlLink(savedChat.ExactName)
                : $"«{chat.Title}»";
        }
    }
}
