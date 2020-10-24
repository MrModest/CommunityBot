using System;
using System.Data.SQLite;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using CommunityBot.Contracts;
using Dapper;

namespace CommunityBot.Persistence
{
    internal class RepositoryBase<TEntity> where TEntity : EntityBase, new()
    {
        protected readonly SQLiteConnection Connection;
        protected readonly string TableName = (Attribute.GetCustomAttribute(typeof(TEntity), typeof(TableAttribute)) as TableAttribute)!.TableName;

        public RepositoryBase(SQLiteConnection connection)
        {
            Connection = connection;
        }
        
        public async Task<TEntity> ById(string id)
        {
            return await Connection.QuerySingleAsync<TEntity>("select * from {tableName} where id = {id}", new { tableName = TableName, id = id });
        }
    }
}