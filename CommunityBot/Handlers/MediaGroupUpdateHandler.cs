using System.Threading.Tasks;
using CommunityBot.Contracts;
using CommunityBot.Handlers.Results;
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
            IOptions<BotConfigurationOptions> options, 
            ILogger<MediaGroupUpdateHandler> logger,
            IMediaGroupService mediaGroupService) : base(options, logger)
        {
            _mediaGroupService = mediaGroupService;
        }

        protected override UpdateType[] AllowedUpdates => new[] {UpdateType.Message};

        protected override bool CanHandle(Update update)
        {
            return update.Message.MediaGroupId.IsNotBlank();
        }

        protected override Task<IUpdateHandlerResult> HandleUpdateInternal(Update update)
        {
            var media = update.Message.ToInputMedia();

            if (media != null)
            {
                _mediaGroupService.AddMediaToGroup(update.Message.MediaGroupId, media);
                return Result.Nothing().AsTask();
            }
            
            Logger.LogInformation("Skipped media for groupId {mediaGroupId} | update: {update}", update.Message.MediaGroupId, update.ToLog());
            
            return Result.Nothing().AsTask();
        }
    }
}
