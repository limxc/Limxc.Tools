using System;
using System.Diagnostics;

namespace Limxc.Tools.Extensions
{
    public static class DebugExtension
    {
        public static void Debug(this string msg)
        {
            if (Debugger.IsAttached)
                System.Diagnostics.Debug.WriteLine($"****** {msg} @ {DateTime.Now:mm:ss fff} ******");
        }

        public static void Debug(this Exception e)
        {
            if (Debugger.IsAttached)
                System.Diagnostics.Debug.WriteLine($"****** {e} @ {DateTime.Now:mm:ss fff} ******");
        }

        public static void Debug<T>(this T obj, Func<T, string> dump = null)
        {
            if (Debugger.IsAttached)
            {
                string msg = dump == null ? obj.ToString() : dump(obj);
                System.Diagnostics.Debug.WriteLine($"****** {msg} @ {DateTime.Now:mm:ss fff} ******");
            }
        }
    }
}