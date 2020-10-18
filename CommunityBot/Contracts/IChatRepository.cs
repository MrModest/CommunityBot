using System.Threading.Tasks;

namespace CommunityBot.Contracts
{
    public interface IChatRepository
    {
        Task<SavedChat?> GetByName(string chatExactName);
        Task<SavedChat?> GetById(long chatId);
        Task<SavedChat> AddOrUpdate(SavedChat savedChat);
        Task<SavedChat?> Remove(string chatExactName);
    }
}