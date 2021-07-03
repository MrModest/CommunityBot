using System.Collections.Generic;
using System.Data.SQLite;
using System.Threading.Tasks;
using CommunityBot.Contracts;

namespace CommunityBot.Persistence
{
    public class SqliteChatRepository : RepositoryBase<SavedChat>, IChatRepository
    {
        public SqliteChatRepository(SQLiteConnection connection) 
            : base(connection)
        {
        }
        
        public Task<IEnumerable<SavedChat>> GetAll()
        {
            return GetAllInternal();
        }

        public Task<SavedChat?> GetByName(string chatExactName)
        {
            return ByField(nameof(SavedChat.ExactName), chatExactName);
        }

        public Task RemoveByName(string name)
        {
            return ExecuteAsync($"DELETE FROM {TableName} where {nameof(SavedChat.ExactName)} = @name", new { name });
        }

        public async Task<bool> AddOrUpdate(SavedChat savedChat)
        {
            var existingEntity = await GetByName(savedChat.ExactName);
            bool isUpdated;

            if (existingEntity is null)
            {
                savedChat.Id = await Insert(savedChat);
                isUpdated = false;
            }
            else
            {
                await Update(savedChat);
                isUpdated = true;
            }

            return isUpdated;
        }
    }
}
