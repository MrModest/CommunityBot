using System;
using System.Linq;
using System.Text;
using System.Threading;
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
    public class RepostMessageUpdateHandler : UpdateHandlerBase
    {
        private const string CreatePostCommand = "event";
        
        private readonly BotConfigurationOptions _options;
        private readonly IChatService _chatService;
        private readonly IMediaGroupService _mediaGroupService;

        public RepostMessageUpdateHandler(
            ITelegramBotClient botClient,
            ILogger<RepostMessageUpdateHandler> logger,
            IOptions<BotConfigurationOptions> options,
            IChatService chatService,
            IMediaGroupService mediaGroupService) : base(botClient, logger)
        {
            _options = options.Value;
            _chatService = chatService;
            _mediaGroupService = mediaGroupService;
        }
        
        protected override UpdateType[] AllowedUpdates => new[] {UpdateType.Message};

        protected override async Task HandleUpdateInternalAsync(Update update)
        {
            Message? message = null;
            
            if (update.Message.GetBotCommands().Any(bc => bc.name == CreatePostCommand))
            {
                message = update.Message;
            }

            if (update.Message.GetMentionedUserNames().Any(u => u == _options.BotName))
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
                    if (string.IsNullOrWhiteSpace(message.MediaGroupId))
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
            
            await BotClient.SendTextMessageAsync(_options.MainChannelId, text, ParseMode.MarkdownV2,
                disableWebPagePreview: true,
                replyMarkup: InlineKeyboardHelper.GetPostButtons());
        }

        private async Task SendPhotoVideoPost(Message message)
        {
            var caption = await PreparePost(message);

            switch (message.Type)
            {
                case MessageType.Photo:
                {
                    var photo = message.Photo.OrderByDescending(ps => ps.FileSize).First().FileId;
                
                    await BotClient.SendPhotoAsync(_options.MainChannelId, photo, caption, ParseMode.MarkdownV2,
                        replyMarkup: InlineKeyboardHelper.GetPostButtons());
                    break;
                }
                case MessageType.Video:
                    await BotClient.SendVideoAsync(_options.MainChannelId, message.Video.FileId,
                        replyMarkup: InlineKeyboardHelper.GetPostButtons());
                    break;
                
                default:
                    throw new InvalidOperationException($"Not supported MessageType: {message.Type}");
            }
        }

        private async Task SendMediaGroupPost(Message message)
        {
            var media = await _mediaGroupService.GetMediaByGroupId(message.MediaGroupId);
            await BotClient.SendMediaGroupAsync(media, _options.MainChannelId);
        }

        private async Task<string> PreparePost(Message message)
        {
            var post = new StringBuilder();
            var postText = message.IsBotCommand() 
                ? message.GetBotCommands().First().arg 
                : message.Text;
            post.Append($"{postText}\n\n");
            post.Append($" — {message.From.GetMentionMdLink()}");
            
            if (!message.IsPrivate())
            {
                var savedChat = await _chatService.GetSavedChat(message.Chat.Id);
                post.Append($" из {savedChat.JoinLink.ToMdLink(savedChat.CustomName ?? savedChat.ExactName)}\n");
                post.Append($" — {message.GetPostLink().ToMdLink("Источник")}");
            }
            
            return post.ToString().EscapeMarkdown();
        }
    }
}