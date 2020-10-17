using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CommunityBot.Contracts;
using Telegram.Bot.Types;

namespace CommunityBot.Services
{
    public class MediaGroupService : IMediaGroupService
    {
        public async Task<IEnumerable<IAlbumInputMedia>> GetMediaByGroupId(string mediaGroupId)
        {
            return Array.Empty<IAlbumInputMedia>();
        }
    }
}