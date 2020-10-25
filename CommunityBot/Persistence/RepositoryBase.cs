using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Threading.Tasks;
using CommunityBot.Contracts;
using Dapper;
using Dapper.Contrib.Extensions;

namespace CommunityBot.Persistence
{
    public abstract class RepositoryBase<TEntity> where TEntity : EntityBase, new()
    {
        private readonly SQLiteConnection _connection;

        protected readonly string TableName =
            (Attribute.GetCustomAttribute(typeof(TEntity), typeof(TableAttribute)) as TableAttribute)!.Name;

        protected RepositoryBase(SQLiteConnection connection)
        {
            _connection = connection;
        }

        protected async Task<TEntity> ById(long id) =>
            await _connection.GetAsync<TEntity>(id);

        protected async Task<TEntity> GetSingle(string query, object parameters) =>
            await _connection.QuerySingleOrDefaultAsync<TEntity>(query, parameters);

        protected async Task<IEnumerable<TEntity>> GetList(string query, object parameters) =>
            await _connection.QueryAsync<TEntity>(query, parameters);
        
        protected async Task Update(TEntity entity) =>
            await _connection.UpdateAsync(entity);

        protected async Task DeleteById(long id) =>
            await _connection.ExecuteAsync($"REMOVE FROM {TableName} WHERE Id = @id", new { id });

        protected async Task ExecuteAsync(string query, object parameters) =>
                await _connection.ExecuteAsync(query, parameters);
    }
}