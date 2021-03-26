using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CommunityBot.Contracts
{
    public interface ILogRepository
    {
        Task<IEnumerable<Log>> ByDate(DateTime? dateTime = null);
    }
}