using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Limxc.Tools.Extensions
{
    public static class RegexExtension
    {
        public static bool IsRegexMatch(this string source,string pattern)
        {
            var suc = false;
            try
            {
                suc = Regex.Match(source, pattern).Value == source;
            }
            catch { }
            return suc;
        }
    }
}
