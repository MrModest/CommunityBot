using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CommunityBot.Contracts;
using Telegram.Bot.Types;

namespace CommunityBot.Services
{
    public class MediaGroupService : IMediaGroupService
    {
        public Task<IEnumerable<IAlbumInputMedia>> GetMediaByGroupId(string mediaGroupId)
        {
            return Task.FromResult<IEnumerable<IAlbumInputMedia>>(Array.Empty<IAlbumInputMedia>());
        }
    }
}