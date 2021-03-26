using System.Collections.Generic;
using System.Data;
using Dapper;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CommunityBot.Helpers
{
    public class JObjectTypeHandler : SqlMapper.TypeHandler<JObject>

    {
        public override void SetValue(IDbDataParameter parameter, JObject value)
        {
            parameter.Value = value.ToString();
        }

        public override JObject Parse(object value)
        {
            var json = value.ToString();
            return JObject.Parse(json);
        }
    }
}