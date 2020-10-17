using System.Threading.Tasks;
using CommunityBot.Contracts;

namespace CommunityBot.Services
{
    public class ChatService : IChatService
    {
        public async Task<SavedChat> GetSavedChat(long chatId)
        {
            return new SavedChat(chatId, "customName", "exactName", "joinLink");
        }
    }
}