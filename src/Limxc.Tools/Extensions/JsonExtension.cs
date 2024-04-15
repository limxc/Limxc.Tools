using Newtonsoft.Json;

namespace Limxc.Tools.Extensions
{
    public static class JsonExtension
    {
        public static string ToJson<T>(this T obj, bool readable = false)
        {
            var json = JsonConvert.SerializeObject(obj, readable ? Formatting.Indented : Formatting.None);
            return json;
        }

        public static T JsonTo<T>(this string json)
        {
            return JsonConvert.DeserializeObject<T>(json);
        }
    }
}