using System;
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
                    @"^" + // 匹配字符串开始
                    @"(https?:\/\/)" + // 协议部分 (http或https)
                    @"(" + // 开始主机部分分组
                    @"(?:" + // 开始域名选项
                    @"(?:[a-zA-Z0-9\u4e00-\u9fa5](?:[a-zA-Z0-9\u4e00-\u9fa5\-]{0,61}[a-zA-Z0-9\u4e00-\u9fa5])?\.)+" + // 子域名部分
                    @"[a-zA-Z\u4e00-\u9fa5]{2,})" + // 顶级域名
                    @"|" + // 或
                    @"(?:" + // 开始IPv4选项
                    @"(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}" + // 前三个IP段
                    @"(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)" + // 最后一个IP段
                    @")" + // 结束主机部分分组
                    @"(?::(0*[1-9]\d{0,3}|[1-5]\d{4}|6[0-4]\d{3}|65[0-4]\d{2}|655[0-2]\d|6553[0-5]))?" +// 端口部分 (可选)
                    @"(\/[a-zA-Z0-9\u4e00-\u9fa5\-._~%!$&'()*+,;=:@\/]*)?" + // 路径部分 (可选)
                    @"(\?[a-zA-Z0-9\u4e00-\u9fa5\-._~%!$&'()*+,;=:@\/?]*)?" + // 查询部分 (可选)
                    @"(#[\w\u4e00-\u9fa5\-._~%!$&'()*+,;=:@\/?]*)?" + // 片段部分 (可选)
                    @"$"; // 匹配字符串结束

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

        /// <summary>
        /// http/https
        /// </summary>
        /// <param name="source"> </param>
        /// <returns> </returns>
        public static bool CheckUrl(this string source)
        {
            return Regex.IsMatch(source, UrlPattern, RegexOptions.IgnoreCase);
        }

        public static bool CheckUri(this string source)
        {
            return Uri.TryCreate(source, UriKind.RelativeOrAbsolute, out var _);
        }
    }
}