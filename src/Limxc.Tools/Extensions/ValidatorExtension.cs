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

        public const string EmailPattern =
            @"^\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$";

        public const string UrlPattern =
            @"(http:\/\/www\.|https:\/\/www\.|http:\/\/|https:\/\/)?[a-z0-9]+([\-\.]{1}[a-z0-9]+)*\.[a-z]{2,5}(:[0-9]{1,5})?(\/.*)?";


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
            return Regex.IsMatch(source, UrlPattern);
        }
    }
}