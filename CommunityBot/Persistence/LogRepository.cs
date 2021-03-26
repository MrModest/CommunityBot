using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CommunityBot.Contracts;
using CommunityBot.Helpers;
using Dapper;
using Microsoft.Extensions.Options;

namespace CommunityBot.Persistence
{
    public class LogRepository : RepositoryBase<Log>, ILogRepository
    {
        public LogRepository(IOptions<SQLiteConfigurationOptions> options)
            : base(options.Value.LogDbFilePath)
        {
            SqlMapper.AddTypeHandler(new JObjectTypeHandler());
        }

        public async Task<IEnumerable<Log>> ByDate(DateTime? dateTime = null)
        {
            dateTime ??= DateTime.Today;

            var dateTo = dateTime.Value.AddHours(29).AddMinutes(59);
            
            return await GetList($"SELECT * FROM {TableName} WHERE Timestamp >= @dateFrom AND Timestamp <= @dateTo", 
                new { dateFrom = dateTime, dateTo = dateTo });
        }
    }
}