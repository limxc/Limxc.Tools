using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Limxc.Tools.Extensions
{
    public static class CommonExtension
    {
        public static string ToJson<T>(this T obj, bool readable = false)
        {
            var json = JsonSerializer.Serialize(obj, new JsonSerializerOptions
            {
                WriteIndented = readable,
                ReferenceHandler = ReferenceHandler.IgnoreCycles,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping //方法1,允许不安全字符(含html)  
                //Encoder = JavaScriptEncoder.Create(System.Text.Unicode.UnicodeRanges.All) // 方法1,允许不安全字符(不含html) 
            });
            return json;
        }

        public static T JsonTo<T>(this string json)
        {
            return JsonSerializer.Deserialize<T>(json);
        }
    }
}