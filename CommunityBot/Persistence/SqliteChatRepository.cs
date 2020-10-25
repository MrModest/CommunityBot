using System.Data.SQLite;
using System.Threading.Tasks;
using CommunityBot.Contracts;

namespace CommunityBot.Persistence
{
    public class SqliteChatRepository : RepositoryBase<SavedChat>, IChatRepository
    {
        protected SqliteChatRepository(SQLiteConnection connection) : base(connection)
        {
        }

        public async Task<SavedChat?> GetByName(string chatExactName)
        {
            return await GetSingle($"SELECT * FROM {TableName} WHERE {nameof(SavedChat.ExactName)} = @name", new { name = chatExactName });
        }

        public async Task RemoveByName(string name)
        {
            await ExecuteAsync($"REMOVE FROM {TableName} where {nameof(SavedChat.ExactName)} = @name", new { name });
        }
        
        public async Task AddOrUpdate(SavedChat savedChat)
        {
            var existingEntity = await ById(savedChat.Id);
            if (existingEntity is null)
            {
                await Add(savedChat);
            }
            else
            {
                await Update(savedChat);
            }
        }

        private async Task Add(SavedChat savedChat)
        {
            await ExecuteAsync($"INSERT INTO {TableName}(" +
                               $"{nameof(SavedChat.Id)}, {nameof(SavedChat.ExactName)}, {nameof(SavedChat.JoinLink)}) " +
                               $"VALUES (@{nameof(SavedChat.Id)}, @{nameof(SavedChat.ExactName)}, @{nameof(SavedChat.JoinLink)})", savedChat);
        }

        private new async Task Update(SavedChat savedChat)
        {
            await base.Update(savedChat);
        }
    }
}