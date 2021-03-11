using System;

namespace Limxc.Tools.Extensions
{
    public static class DebugExtension
    {
        public static void Debug(this string msg)
        {
            Console.WriteLine($"****** {msg} @ {DateTime.Now:mm:ss fff} ******");
        }

        public static void Debug(this Exception e)
        {
            Console.WriteLine($"****** {e} @ {DateTime.Now:mm:ss fff} ******");
        }

        public static void Debug<T>(this T obj, Func<T, string> dump = null)
        {
            var msg = dump == null ? obj.ToString() : dump(obj);
            Console.WriteLine($"****** {msg} @ {DateTime.Now:mm:ss fff} ******");
        }
    }
}