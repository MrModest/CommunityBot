using System;
using System.Collections.Generic;
using System.Data.SQLite;
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

        protected RepositoryBase(SQLiteConnection connection)
        {
            _connection = connection;
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
    }
}