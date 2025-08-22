using FluentAssertions;
using Limxc.Tools.Extensions;
using Xunit;

namespace Limxc.ToolsTests.Extensions;

public class ValidatorExtensionTests
{
    [Fact]
    public void CheckIpPortTest()
    {
        "0.0.0.0:0".CheckIpPort().Should().BeTrue();
        "10.20.30.40:50".CheckIpPort().Should().BeTrue();
        "255.255.255.255:65535".CheckIpPort().Should().BeTrue();

        "-1.0.0.0:0".CheckIpPort().Should().BeFalse();
        "255.255.255.255:-1".CheckIpPort().Should().BeFalse();
        "255.255.255.255:65536".CheckIpPort().Should().BeFalse();
        "256.255.255.255:65535".CheckIpPort().Should().BeFalse();

        "255.255.255.255: 65535".CheckIpPort().Should().BeFalse();
        "255.255 .255.255:65535".CheckIpPort().Should().BeFalse();
    }

    [Fact]
    public void CheckIpTest()
    {
        "0.0.0.0".CheckIp().Should().BeTrue();
        "10.20.30.40".CheckIp().Should().BeTrue();
        "255.255.255.255".CheckIp().Should().BeTrue();

        "-1.0.0.0".CheckIp().Should().BeFalse();
        "256.254.255.255".CheckIp().Should().BeFalse();
        "255.256.255.255".CheckIp().Should().BeFalse();
        "255.255.256.255".CheckIp().Should().BeFalse();
        "255.255.256.256".CheckIp().Should().BeFalse();

        "255.255 .255.255".CheckIp().Should().BeFalse();
    }

    [Fact]
    public void CheckPortTest()
    {
        "-1".CheckPort().Should().BeFalse();
        "65536".CheckPort().Should().BeFalse();

        "4".CheckPort().Should().BeTrue();
        "25".CheckPort().Should().BeTrue();
        "254".CheckPort().Should().BeTrue();
        "6625".CheckPort().Should().BeTrue();
    }

    [Fact]
    public void CheckEmailTest()
    {
        "xxx@asd.com".CheckEmail().Should().BeTrue();

        " xxx@asd.com ".CheckEmail().Should().BeFalse();
        "xxx.asd.com".CheckEmail().Should().BeFalse();
        "xxx.asd@com".CheckEmail().Should().BeFalse();
        "xxxasd.com".CheckEmail().Should().BeFalse();
    }

    [Fact]
    public void CheckUrlTest()
    {
        string[] validUrls = {
            // 基本HTTP/HTTPS
            "http://example.com",
            "https://example.com",

            // 带有效端口的URL
            "http://example.com:80",
            "https://example.com:443",
            "http://example.com:8080",
            "https://example.com:3000",
            "http://example.com:65535", // 最大有效端口

            // 带路径的URL
            "http://example.com/path",
            "https://example.com/path/to/resource",
            "http://example.com/目录/页面.html",

            // 带查询参数的URL
            "http://example.com?key=value",
            "https://example.com?param1=value1&param2=value2",
            "http://example.com?查询=值&参数=数据",

            // 带片段标识符的URL
            "http://example.com#section",
            "https://example.com/path#片段",

            // 完整URL（包含所有部分）
            "https://example.com:8080/path?query=value#fragment",
            "http://example.com:3000/路径?查询=值#片段标识",

            // IPv4地址
            "http://192.168.1.1",
            "https://8.8.8.8:53",
            "http://127.0.0.1:3000/api",
            "https://255.255.255.255:8080", // 最大IP值

            // 中文域名
            "http://中文.测试",
            "https://例子.中国",
            "http://示例.公司.cn",

            // 特殊字符路径和参数
            "http://example.com/path-with-dashes",
            "https://example.com/path_with_underscores",
            "http://example.com/path%20with%20spaces",
            "https://example.com?key-with-dash=value",
            "http://example.com?key_with_underscore=value",

            // 边缘情况
            "http://a.b.c.d.example.com", // 多级子域名
            "https://example.com:1", // 最小有效端口
            "http://example.com/path?query=value&another=param" // 多个查询参数
        };

        string[] invalidUrls = {
            // 缺少协议
            "example.com",
            "www.example.com/path",

            // 无效协议
            "ftp://example.com",
            "file:///path/to/file",
            "ws://example.com",

            // 协议格式错误
            "http//example.com",
            "https:example.com",
            "http:://example.com",

            // 无效IPv4地址
            "http://256.1.1.1",          // 数字超过255
            "https://192.168.1",         // 不完整的IP
            "http://192.168.1.1.1",      // 过多的数字段
            "https://192.168.1.256",     // 数字超过255

            // 无效端口
            "http://example.com:0",      // 端口0
            "https://example.com:65536", // 端口超过最大值
            "http://example.com:abc",    // 非数字端口
            "https://example.com:-1",    // 负端口
            "http://example.com:00000",  // 端口0的多零表示

            // 无效域名格式
            "http://.example.com",       // 空标签
            "https://example..com",      // 连续点
            "http://-example.com",       // 以连字符开头
            "https://example-.com",      // 以连字符结尾

            // 无效字符
            "http://example.com/path with spaces", // 未编码的空格
            "https://example.com/path[with]brackets", // 方括号
            "http://example.com/path{with}braces", // 花括号

            // 不完整的URL
            "http://",
            "https://:8080",
            "http://?query=value",

            // 其他无效格式
            "just a string",
            "",
            "https://",
            "http:///path",

            // 混合字符域名
            "http://examplé-test.com",
            "https://café.example.org",

            // 特殊无效情况
            "http://example.com:00",     // 端口00
            "https://example.com:0000",  // 端口0000
            "http://999.999.999.999:8080", // 无效IP加有效端口
            "https://example.com:65536/path" // 超大端口加路径
        };

        foreach (var s in validUrls)
            s.CheckUrl().Should().BeTrue($"失败:{s}");

        foreach (var s in invalidUrls)
            s.CheckUrl().Should().BeFalse($"失败:{s}");
    }
}