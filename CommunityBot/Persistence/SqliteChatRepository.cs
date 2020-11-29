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

        public async Task<SavedChat?> GetByName(string chatExactName)
        {
            return await GetSingle($"SELECT * FROM {TableName} WHERE {nameof(SavedChat.ExactName)} = @name", new { name = chatExactName });
        }

        public async Task RemoveByName(string name)
        {
            await ExecuteAsync($"DELETE FROM {TableName} where {nameof(SavedChat.ExactName)} = @name", new { name });
        }

        public async Task AddOrUpdate(SavedChat savedChat)
        {
            var existingEntity = await GetByName(savedChat.ExactName);
            if (existingEntity is null)
            {
                savedChat.Id = await Insert(savedChat);
            }
            else
            {
                await Update(savedChat);
            }
        }
    }
}