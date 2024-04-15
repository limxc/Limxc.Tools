using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Limxc.Tools.Extensions
{
    public static class JsonExtension
    {
        public static JsonSerializerOptions Init(this JsonSerializerOptions options, bool readable)
        {
            options.Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
            options.WriteIndented = readable;
            options.ReferenceHandler = ReferenceHandler.IgnoreCycles;

            options.ReadCommentHandling = JsonCommentHandling.Skip;
            options.AllowTrailingCommas = true;

            options.NumberHandling = JsonNumberHandling.AllowReadingFromString | JsonNumberHandling.WriteAsString;

            return options;
        }

        public static string ToJson<T>(this T obj, bool readable = false)
        {
            var json = JsonSerializer.Serialize(obj, new JsonSerializerOptions().Init(readable));
            return json;
        }

        public static T JsonTo<T>(this string json)
        {
            return JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions().Init(false));
        }
    }
}