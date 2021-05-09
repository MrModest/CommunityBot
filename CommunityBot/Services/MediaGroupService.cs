using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using CommunityBot.Contracts;
using Telegram.Bot.Types;

namespace CommunityBot.Services
{
    public class MediaGroupService : IMediaGroupService
    {
        private readonly IMemoryCacheWrapper _memoryCache;
        
        public MediaGroupService(
            IMemoryCacheWrapperFactory memoryCacheWrapperFactory)
        {
            _memoryCache = memoryCacheWrapperFactory.CreateWrapper("MediaGroupId");
        }

        public void AddMediaToGroup(string mediaGroupId, IAlbumInputMedia media)
        {
            if (_memoryCache.TryGetValue(mediaGroupId, out ConcurrentBag<IAlbumInputMedia> entry))
            {
                entry.Add(media);
            }
            else
            {
                _memoryCache.Set(mediaGroupId, new ConcurrentBag<IAlbumInputMedia> {media}, TimeSpan.FromDays(1));
            }
        }

        public IEnumerable<IAlbumInputMedia>? GetMediaByGroupId(string mediaGroupId)
        {
            return _memoryCache.TryGetValue(mediaGroupId, out ConcurrentBag<IAlbumInputMedia> entry)
                ? entry.ToArray()
                : null;
        }
    }
}
