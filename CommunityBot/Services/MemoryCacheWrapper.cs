using System;
using CommunityBot.Contracts;
using Microsoft.Extensions.Caching.Memory;

namespace CommunityBot.Services
{
    public class MemoryCacheWrapper : IMemoryCacheWrapper
    {
        private readonly string _prefix;
        private readonly IMemoryCache _memoryCache;

        public MemoryCacheWrapper(IMemoryCache memoryCache, string prefix)
        {
            _prefix = prefix;
            _memoryCache = memoryCache;
        }
        
        public TItem Set<TItem>(string key, TItem value, TimeSpan? absoluteExpirationRelativeToNow = null)
        {
            if (absoluteExpirationRelativeToNow == null)
            {
                return _memoryCache.Set($"{_prefix}{key}", value);
            }
            
            return _memoryCache.Set($"{_prefix}{key}", value, absoluteExpirationRelativeToNow.Value);
        }

        public bool TryGetValue<TItem>(string key, out TItem value)
        {
            if (_memoryCache.TryGetValue($"{_prefix}{key}", out TItem result))
            {
                value = result;
                return true;
            }

            value = default;
            return false;
        }
    }
}
