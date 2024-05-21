using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Limxc.Tools.Extensions
{
    public static class JsonExtension
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor,
            ObjectCreationHandling = ObjectCreationHandling.Replace,
            //DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate,
            MissingMemberHandling = MissingMemberHandling.Ignore,
            //NullValueHandling = NullValueHandling.Ignore,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            //DateFormatHandling = DateFormatHandling.IsoDateFormat,
            //DateFormatString = "yyyy-MM-dd HH:mm:ss",

            //FloatFormatHandling = FloatFormatHandling.String,
            FloatParseHandling = FloatParseHandling.Decimal,

            Converters = new List<JsonConverter>
            {
                new StringEnumConverter()
            }
        };

        public static string ToJson<T>(this T obj, bool readable = false)
        {
            var json = JsonConvert.SerializeObject(obj, readable ? Formatting.Indented : Formatting.None, Settings);
            return json;
        }

        public static T JsonTo<T>(this string json)
        {
            return JsonConvert.DeserializeObject<T>(json, Settings);
        }
    }
}