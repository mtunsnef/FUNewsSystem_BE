using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace FUNewsSystem.Domain.Extensions.System
{
    public static class JsonSerializationExtensions
    {
        public static string ToJson(this object obj)
        {
            var serializeOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            return JsonSerializer.Serialize(obj, serializeOptions);
        }
    }
}
