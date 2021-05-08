using System;
using System.Collections.Generic;
using System.Linq;

namespace CommunityBot.Handlers.Results
{
    public class AggregateUpdateHandlerResult : IUpdateHandlerResult
    {
        public AggregateUpdateHandlerResult(IEnumerable<IUpdateHandlerResult> innerResults)
        {
            InnerResults = innerResults.ToArray();
            
            if (InnerResults.OfType<AggregateUpdateHandlerResult>().Any())
            {
                throw new ArgumentException("Nesting is not allowed!");
            }
        }

        public IUpdateHandlerResult[] InnerResults { get; }
    }
}