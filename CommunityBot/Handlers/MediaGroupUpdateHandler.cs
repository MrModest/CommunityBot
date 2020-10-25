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
    public class MediaGroupUpdateHandler : UpdateHandlerBase
    {
        private readonly IMediaGroupService _mediaGroupService;

        public MediaGroupUpdateHandler(
            ITelegramBotClient botClient, 
            IOptions<BotConfigurationOptions> options, 
            ILogger<MediaGroupUpdateHandler> logger,
            IMediaGroupService mediaGroupService) : base(botClient, options, logger)
        {
            _mediaGroupService = mediaGroupService;
        }

        protected override UpdateType[] AllowedUpdates => new[] {UpdateType.Message};

        protected override bool CanHandle(Update update)
        {
            return !string.IsNullOrWhiteSpace(update.Message.MediaGroupId);
        }

        protected override async Task HandleUpdateInternalAsync(Update update)
        {
            IAlbumInputMedia? media = update.Message.Type switch
            {
                MessageType.Photo => update.Message.Photo.ToInputMedia(),
                MessageType.Video => update.Message.Video.ToInputMedia(),
                _ => null
            };

            if (media != null)
            {
                _mediaGroupService.AddMediaToGroup(update.Message.MediaGroupId, media);
            }
        }
    }
}