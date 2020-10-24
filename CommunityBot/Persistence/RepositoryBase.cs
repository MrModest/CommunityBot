using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Threading.Tasks;
using CommunityBot.Contracts;
using Dapper;

namespace CommunityBot.Persistence
{
    internal class RepositoryBase<TEntity> where TEntity : EntityBase, new()
    {
        private readonly SQLiteConnection _connection;
        protected readonly string TableName = (Attribute.GetCustomAttribute(typeof(TEntity), typeof(TableAttribute)) as TableAttribute)!.TableName;

        public RepositoryBase(SQLiteConnection connection)
        {
            _connection = connection;
        }
        
        protected async Task<TEntity> ById(string id)
        {
            return await _connection.QuerySingleOrDefaultAsync<TEntity>("select * from {tableName} where id = {id}", new { tableName = TableName, id });
        }

        protected async Task<TEntity> GetSingle(string query, object parameters)
        {
            return await _connection.QuerySingleOrDefaultAsync<TEntity>(query, CreateQueryParams(parameters));
        }

        protected async Task<IEnumerable<TEntity>> GetList(string query, object parameters)
        {
            return await _connection.QueryAsync<TEntity>(query, CreateQueryParams(parameters));
        }

        protected async Task<IEnumerable<TEntity>> Insert(TEntity entity)
        {
            return await _connection.QueryAsync<TEntity>("insert into {tableName} name values {values}",
                CreateQueryParams(entity));
        }
        
        /// <summary>
        /// Enriches parameters with params lite table name of db name
        /// </summary>
        /// <param name="parameters">object with query parameters</param>
        /// <returns>enriched parameters object</returns>
        private object CreateQueryParams(dynamic parameters)
        {
            parameters.tableName = TableName;
            return parameters;
        }
    }
}