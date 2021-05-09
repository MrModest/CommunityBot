using CommunityBot.Contracts;
using Microsoft.Extensions.Caching.Memory;

namespace CommunityBot.Services
{
    public class MemoryCacheWrapperFactory : IMemoryCacheWrapperFactory
    {
        private readonly IMemoryCache _memoryCache;

        public MemoryCacheWrapperFactory(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }

        public IMemoryCacheWrapper CreateWrapper(string prefix)
        {
            return new MemoryCacheWrapper(_memoryCache, prefix);
        }
    }
}
