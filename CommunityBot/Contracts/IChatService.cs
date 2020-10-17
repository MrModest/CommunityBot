using System.Threading.Tasks;

namespace CommunityBot.Contracts
{
    public interface IChatService
    {
        Task<SavedChat> GetSavedChat(long chatId);
    }
}