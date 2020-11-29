using System.Collections.Generic;
using System.Threading.Tasks;

namespace CommunityBot.Contracts
{
    public interface IChatRepository
    {
        Task<IEnumerable<SavedChat>> GetAll();
        Task AddOrUpdate(SavedChat entity);
        Task<SavedChat?> GetByName(string chatExactName);
        Task RemoveByName(string name);
    }
}