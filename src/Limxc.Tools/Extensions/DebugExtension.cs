using System;

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
    }
}