using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

using CommunityBot.Contracts;
using CommunityBot.Handlers.Results;
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

        protected override async Task<IUpdateHandlerResult> HandleUpdateInternal(Update update)
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
                return await SendPost(message);
            }
            
            Logger.LogInformation("Update {Update} was skipped!", update.ToLog());

            return Result.Nothing();
        }

        private async Task<IUpdateHandlerResult> SendPost(Message message)
        {
            switch (message.Type)
            {
                case MessageType.Text:
                    return await SendTextPost(message);
                case MessageType.Photo:
                case MessageType.Video:
                    if (message.MediaGroupId.IsBlank())
                    {
                        return await SendPhotoVideoPost(message);    
                    }
                    else
                    {
                        return await SendMediaGroupPost(message);
                    }
                
                default:
                    return Result.Nothing();
            }
        }

        private async Task<IUpdateHandlerResult> SendTextPost(Message message)
        {
            var text = await PreparePost(message);
            
            return Result.Text(Options.MainChannelId, text, ParseMode.Html, true);
        }

        private async Task<IUpdateHandlerResult> SendPhotoVideoPost(Message message)
        {
            var caption = await PreparePost(message);

            switch (message.Type)
            {
                case MessageType.Photo:
                {
                    var photo = message.Photo.GetLargestPhotoSize().FileId;
                
                    return Result.Photo(Options.MainChannelId, photo, caption, ParseMode.Html);
                }
                case MessageType.Video:
                    return Result.Video(Options.MainChannelId, message.Video.FileId, caption, ParseMode.Html);
                
                default:
                    throw new InvalidOperationException($"Not supported MessageType: {message.Type}");
            }
        }

        private async Task<IUpdateHandlerResult> SendMediaGroupPost(Message message)
        {
            var media = _mediaGroupService.GetMediaByGroupId(message.MediaGroupId)?.ToArray();

            if (media == null)
            {
                Logger.LogWarning("Post was not send because not found media group with id '{Id}'", message.MediaGroupId);
                return Result.Nothing();
            }

            foreach (var inputMedia in media.Where(m => m.Caption != null).OfType<InputMediaBase>())
            {
                message.Caption = inputMedia.Caption;
                inputMedia.Caption = await PreparePost(message);
                inputMedia.ParseMode = ParseMode.Html;
            }
            
            return Result.MediaGroup(Options.MainChannelId, media);
        }

        private async Task<string> PreparePost(Message message)
        {
            var post = new StringBuilder();
            var postText = message.GetFirstBotCommand()?.arg ?? message.Text ?? message.Caption;

            postText = postText.EncodeHtml();

            if (message.GetEntities().Any())
            {
                postText = MessageEntityWrapper.GetMarkupMessage(
                    postText,
                    message.GetEntities(), 
                    ParseMode.Html);
            }

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
