#define DEBUG  
using System;
using System.Text.Json;

namespace Limxc.Tools.Extensions
{
    public static class DebugExtension
    {
 
        public static void Debug(this Exception e, bool console = false)
        {
            if (console)
                Console.WriteLine($"****** {e} @ {DateTime.Now:mm:ss fff} ******");
            else
                System.Diagnostics.Debug.WriteLine($"****** {e} @ {DateTime.Now:mm:ss fff} ******");
        }

        public static void Debug<T>(this T obj, bool console = false, Func<T, string> dump = null)
        {
            var msg = dump == null ? obj.ToString() : dump(obj);
            if (console)
                Console.WriteLine($"****** {msg} @ {DateTime.Now:mm:ss fff} ******");
            else
                System.Diagnostics.Debug.WriteLine($"****** {msg} @ {DateTime.Now:mm:ss fff} ******");
        }

        public static string Dump<T>(this T obj, bool console = false)
        {
            var msg = JsonSerializer.Serialize(obj, new JsonSerializerOptions
            {
                WriteIndented = true,
                IncludeFields = true
            });

            var name = typeof(T).FullName;

            if (console)
                Console.WriteLine($"****** {name} @ {DateTime.Now:mm:ss fff} ******{Environment.NewLine}{msg}");
            else
                System.Diagnostics.Debug.WriteLine(
                    $"****** {name} @ {DateTime.Now:mm:ss fff} ******{Environment.NewLine}{msg}");

            return msg;
        }
    }
}