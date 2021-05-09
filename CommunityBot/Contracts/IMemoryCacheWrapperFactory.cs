namespace CommunityBot.Contracts
{
    public interface IMemoryCacheWrapperFactory
    {
        IMemoryCacheWrapper CreateWrapper(string prefix);
    }
}
