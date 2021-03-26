using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Threading.Tasks;
using CommunityBot.Contracts;
using Dapper;
using Dapper.Contrib.Extensions;

namespace CommunityBot.Persistence
{
    public abstract class RepositoryBase<TEntity> where TEntity : class, IEntity, new()
    {
        private readonly SQLiteConnection _connection;

        protected readonly string TableName =
            (Attribute.GetCustomAttribute(typeof(TEntity), typeof(TableAttribute)) as TableAttribute)!.Name;

        protected RepositoryBase(string dbFilePath)
        {
            _connection = new SQLiteConnection($"DataSource=\"{dbFilePath}\";");
            _connection.Open();
            EnsureDatabase(_connection);
        }
        
        protected async Task<IEnumerable<TEntity>> GetAllInternal() =>
            await _connection.GetAllAsync<TEntity>();

        protected Task<IEnumerable<TEntity>> GetPageInternal(int pageNum, int pageSize = 10) =>
            GetList($"SELECT * FROM {TableName} LIMIT @pageSize OFFSET @offset", new {pageSize, offset = pageNum * pageSize});

        protected async Task<TEntity?> ById(long id) =>
            await _connection.GetAsync<TEntity>(id);
        
        protected Task<TEntity?> ByField(string fieldName, string fieldValue) =>
            GetSingleOrDefault($"SELECT * FROM {TableName} WHERE {fieldName} = @fieldValue", new {fieldValue});

        protected async Task<TEntity?> GetSingleOrDefault(string query, object parameters) =>
            await _connection.QuerySingleOrDefaultAsync<TEntity>(query, parameters);

        protected async Task<IEnumerable<TEntity>> GetList(string query, object parameters) =>
            await _connection.QueryAsync<TEntity>(query, parameters);

        protected async Task<long> Insert(TEntity entity) =>
            await _connection.InsertAsync(entity);
        
        protected async Task Update(TEntity entity) =>
            await _connection.UpdateAsync(entity);

        protected async Task DeleteById(long id) =>
            await _connection.ExecuteAsync($"REMOVE FROM {TableName} WHERE Id = @id", new { id });

        protected async Task ExecuteAsync(string query, object parameters) =>
                await _connection.ExecuteAsync(query, parameters);
        
        private static void EnsureDatabase(SQLiteConnection connection)
        {
            var appTableNames = new[] {"SavedChats", "Users"};
            
            var tableNames = connection.Query<string>("SELECT name FROM sqlite_master WHERE type='table';");

            foreach (var tableName in appTableNames.Except(tableNames))
            {
                if (tableName == "SavedChats")
                {
                    connection.Execute(@"CREATE TABLE IF NOT EXISTS main.SavedChats (
                                            Id        INT  NOT NULL PRIMARY KEY, 
                                            ChatId    INT  NOT NULL, 
                                            ExactName TEXT NOT NULL, 
                                            JoinLink  TEXT NOT NULL);");
                    connection.Execute("CREATE INDEX IF NOT EXISTS main.ExactName_desc ON SavedChats (ExactName DESC);");
                }

                if (tableName == "Users")
                {
                    connection.Execute(@"CREATE TABLE IF NOT EXISTS main.Users (
                                            Id            INT  NOT NULL PRIMARY KEY, 
                                            Username      TEXT DEFAULT NULL, 
                                            FirstName     TEXT DEFAULT NULL, 
                                            LastName      TEXT DEFAULT NULL, 
                                            InvitedBy     INT  DEFAULT NULL, 
                                            InviteComment TEXT DEFAULT NULL, 
                                            AccessType    TEXT NOT NULL,
                                            PasswordHash  TEXT DEFAULT NULL);");
                    
                    connection.Execute("CREATE INDEX IF NOT EXISTS main.Username_desc   ON Users (Username DESC);");
                    connection.Execute("CREATE INDEX IF NOT EXISTS main.AccessType_desc ON Users (AccessType DESC);");
                }
            }
        }
    }
}