﻿using FluentAssertions;
using Xunit;

namespace Limxc.Tools.Extensions.Tests
{
    public class StringExtensionTests
    {
        [Fact()]
        public void ContainsTest()
        {
            "中文English!@#".Contains(new string[] { "天" }).Should().BeFalse();
            "中文English!@#".Contains(new string[] { "天", "中1" }).Should().BeFalse();
            "中文English!@#".Contains(new string[] { "天", "En1" }).Should().BeFalse();

            "中文English!@#".Contains(new string[] { "天", "中" }).Should().BeTrue();
            "中文English!@#".Contains(new string[] { "天", "En" }).Should().BeTrue();

            "中文English!@#".Contains(new string[] { "天", "en" }, false).Should().BeFalse();
        }

        [Fact()]
        public void DeleteChineseWordTest()
        {
            "中文English!@#".DeleteChineseWord().Should().Be("English!@#");
        }

        [Fact()]
        public void RetainChineseWordTest()
        {
            "中文English!@#".RetainChineseWord().Should().Be("中文");
        }

        [Fact()]
        public void NumbersTest()
        {
            "身高:166,体重:55.55,年龄:22".Numbers().Should().BeEquivalentTo(166, 55.55, 22);
        }
    }
}