using System;

namespace CommunityBot.Contracts
{
    public interface IMemoryCacheWrapper
    {
        TItem Set<TItem>(string key, TItem value, TimeSpan? absoluteExpirationRelativeToNow = null);
        bool TryGetValue<TItem>(string key, out TItem value);
    }
}
