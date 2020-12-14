using System.Text.RegularExpressions;

namespace Limxc.Tools.Extensions
{
    public static class ValidatorExtension
    {
        private static bool IsRegexMatch(this string source, string pattern)
        {
            var suc = false;
            try
            {
                suc = Regex.Match(source, pattern).Value == source;
            }
            catch { }
            return suc;
        }

        public static bool CheckIp(this string str)
            => str.IsRegexMatch(@"\b(?:(?:2(?:[0-4][0-9]|5[0-5])|[0-1]?[0-9]?[0-9])\.){3}(?:(?:2([0-4][0-9]|5[0-5])|[0-1]?[0-9]?[0-9]))\b");

        public static bool CheckPort(this string str)
            => str.IsRegexMatch(@"^([0-9]{1,4}|[1-5][0-9]{4}|6[0-4][0-9]{3}|65[0-4][0-9]{2}|655[0-2][0-9]|6553[0-5])$");

        public static bool CheckIpPort(this string str)
            => str.IsRegexMatch(@"\b(?:(?:2(?:[0-4][0-9]|5[0-5])|[0-1]?[0-9]?[0-9])\.){3}(?:(?:2([0-4][0-9]|5[0-5])|[0-1]?[0-9]?[0-9]))\b:{1}([0-9]{1,4}|[1-5][0-9]{4}|6[0-4][0-9]{3}|65[0-4][0-9]{2}|655[0-2][0-9]|6553[0-5])$");

        public static bool CheckEmail(this string str)
            => str.IsRegexMatch(@"^\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$");
         
    }
}