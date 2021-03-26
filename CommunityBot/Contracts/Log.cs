using System;
using System.Collections.Generic;
using Dapper.Contrib.Extensions;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

namespace CommunityBot.Contracts
{
    [Table("Logs")]
    public class Log : IEntity
    {
        public long Id { get; set; }

        public DateTime Timestamp { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public LogLevel Level { get; set; }

        public string Exception { get; set; }

        public string RenderedMessage { get; set; }

        public JObject Properties { get; set; }
    }
}