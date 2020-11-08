using System.Collections.Generic;
using Telegram.Bot.Types;

namespace CommunityBot.Contracts
{
    public interface IMediaGroupService
    {
        IEnumerable<IAlbumInputMedia>? GetMediaByGroupId(string mediaGroupId);

        void AddMediaToGroup(string mediaGroupId, IAlbumInputMedia media);
    }
}