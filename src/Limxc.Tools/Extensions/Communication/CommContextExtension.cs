using System.Text.RegularExpressions;

namespace Limxc.Tools.Extensions.Communication
{
    public static class CommContextExtension
    {
        /// <summary>
        ///     计算指令长度,$n 0-9位
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="sep"></param>
        public static int TemplateLength(this string cmd, char sep = '$')
        {
            try
            {
                cmd = cmd.Replace(" ", "");
                var arr = cmd.ToCharArray();
                var totalLen = arr.Length;
                for (var i = 0; i < arr.Length - 1; i++)
                    if (arr[i] == sep)
                    {
                        var len = arr[i + 1].ToString().HexToInt();
                        len = len > 1 ? len - 1 : 0;
                        totalLen += len * 2;
                    }

                return totalLen;
            }
            catch
            {
                return 0;
            }
        }

        public static bool IsTemplateMatch(this string template, string resp, char sep = '$', bool restrict = true)
        {
            if (string.IsNullOrWhiteSpace(template))
                return string.IsNullOrWhiteSpace(resp);

            if (string.IsNullOrWhiteSpace(resp))
                return false;

            template = template.Replace(" ", "");
            resp = resp.Replace(" ", "");

            var regexStr = string.Empty;

            foreach (var item in template.ToStrArray(2))
                if (item[0] == sep)
                    regexStr += $"[0-9a-fA-F]{{{item[1].ToString().HexToInt() * 2}}}";
                else
                    regexStr += item;

            if (restrict)
                regexStr = $"^{regexStr}$";

            return Regex.IsMatch(resp, regexStr, RegexOptions.IgnoreCase);
        }

        public static string TryGetTemplateMatchResult(this string template, string resp, char sep = '$')
        {
            if (string.IsNullOrWhiteSpace(template) || string.IsNullOrWhiteSpace(resp))
                return string.Empty;

            template = template.Replace(" ", "");
            resp = resp.Replace(" ", "");

            var regexStr = string.Empty;

            foreach (var item in template.ToStrArray(2))
                if (item[0] == sep)
                    regexStr += $"[0-9a-fA-F]{{{item[1].ToString().HexToInt() * 2}}}";
                else
                    regexStr += item;

            return Regex.Match(resp, regexStr, RegexOptions.IgnoreCase).Value;
        }
    }
}