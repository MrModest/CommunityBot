using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityBot.Contracts;
using Microsoft.Extensions.Caching.Memory;
using Telegram.Bot.Types;

namespace CommunityBot.Services
{
    public class MediaGroupService : IMediaGroupService
    {
        private readonly IMemoryCache _memoryCache;
        
        public MediaGroupService(
            IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
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