using System.Threading.Tasks;

namespace CommunityBot.Contracts
{
    public interface IChatRepository : IRepositoryBase<SavedChat>
    {
        Task<SavedChat?> GetByName(string chatExactName);
        Task RemoveByName(string name);
    }
}