using System.Text.RegularExpressions;

namespace Limxc.Tools.Extensions
{
    public static class ValidatorExtension
    {
        public const string IpPattern =
            @"^(?:(?:2(?:[0-4][0-9]|5[0-5])|[0-1]?[0-9]?[0-9])\.){3}(?:(?:2([0-4][0-9]|5[0-5])|[0-1]?[0-9]?[0-9]))$";

        public const string PortPattern =
            @"^([0-9]{1,4}|[1-5][0-9]{4}|6[0-4][0-9]{3}|65[0-4][0-9]{2}|655[0-2][0-9]|6553[0-5])$";

        public const string IpPortPattern =
            @"^(?:(?:2(?:[0-4][0-9]|5[0-5])|[0-1]?[0-9]?[0-9])\.){3}(?:(?:2([0-4][0-9]|5[0-5])|[0-1]?[0-9]?[0-9]))\b:{1}([0-9]{1,4}|[1-5][0-9]{4}|6[0-4][0-9]{3}|65[0-4][0-9]{2}|655[0-2][0-9]|6553[0-5])$";

        public const string EmailPattern = @"^\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$";

        public const string UrlPattern =
            @"^((https?|ftp|file):\/\/)?"
            + // protocol
            @"(([a-z0-9$_\.\+!\*\'\(\),;\?&=-]|%[0-9a-f]{2})+"
            + // username
            @"(:([a-z0-9$_\.\+!\*\'\(\),;\?&=-]|%[0-9a-f]{2})+)?"
            + // password
            @"@)?(?#"
            + // auth requires @
            @")((([a-z0-9]\.|[a-z0-9][a-z0-9-_]*[a-z0-9]\.)*"
            + // domain segments AND
            @"[a-z][a-z0-9-]*[a-z0-9]"
            + // top level domain  OR
            @"|((\d|[1-9]\d|1\d{2}|2[0-4][0-9]|25[0-5])\.){3}"
            + @"(\d|[1-9]\d|1\d{2}|2[0-4][0-9]|25[0-5])"
            + // IP address
            @")(:\d+)?"
            + // port
            @")(((\/+([a-z0-9$_\.\+!\*\'\(\),;:@&=-]|%[0-9a-f]{2})*)*"
            + // path
            @"(\?([a-z0-9$_\.\+!\*\'\(\),;:@&=-]|%[0-9a-f]{2})*)"
            + // query string
            @"?)?)?"
            + // path and query string optional
            @"(#([a-z0-9$_\.\+!\*\'\(\),;:@&=-]|%[0-9a-f]{2})*)?"
            + // fragment
            @"$";

        public static bool CheckIp(this string source)
        {
            return Regex.IsMatch(source, IpPattern);
        }

        public static bool CheckPort(this string source)
        {
            return Regex.IsMatch(source, PortPattern);
        }

        public static bool CheckIpPort(this string source)
        {
            return Regex.IsMatch(source, IpPortPattern);
        }

        public static bool CheckEmail(this string source)
        {
            return Regex.IsMatch(source, EmailPattern);
        }

        public static bool CheckUrl(this string source)
        {
            return Regex.IsMatch(source, UrlPattern, RegexOptions.IgnoreCase);
        }
    }
}