using System.Collections.Generic;
using System.Threading.Tasks;

namespace CommunityBot.Contracts
{
    public interface IChatRepository
    {
        Task<IEnumerable<SavedChat>> GetAll();
        /// <returns>True if chat link was updated and false otherwise.</returns>
        Task<bool> AddOrUpdate(SavedChat entity);
        Task<SavedChat?> GetByName(string chatExactName);
        Task RemoveByName(string name);
    }
}
