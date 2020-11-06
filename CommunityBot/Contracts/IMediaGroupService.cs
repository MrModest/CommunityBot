using System.Collections.Generic;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace CommunityBot.Contracts
{
    public interface IMediaGroupService
    {
        Task<IEnumerable<IAlbumInputMedia>?> GetMediaByGroupId(string mediaGroupId);

        void AddMediaToGroup(string mediaGroupId, IAlbumInputMedia media);
    }
}