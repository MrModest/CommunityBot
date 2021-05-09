using System.Data.SQLite;
using System.Threading.Tasks;
using Dapper;

namespace CommunityBot.Persistence
{
    public class MigrationRepository
    {
        private readonly SQLiteConnection _connection;

        public MigrationRepository(SQLiteConnection connection)
        {
            _connection = connection;
        }

        public async Task<int> ExecuteMigration(string query, object? parameters = null)
        {
            return await _connection.ExecuteAsync(query.Trim(), parameters);
        }
    }
}
