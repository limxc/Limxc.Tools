using FluentAssertions;
using Microsoft.Reactive.Testing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using Xunit;

namespace Limxc.Tools.Extensions.Tests
{
    public class RxExtensionTests
    {
        /*
            rx不适合处理大量实时数据 
         */

        [Fact()]
        public void CharBufferUntilTest()
        {
            var str = "1234aa0000bbaa1111bb4321";
            var list = new List<char>();

            var ts = new TestScheduler();

            var obs = Observable.Create<char>(x =>
                    {
                        foreach (var c in str.ToCharArray())
                        {
                            x.OnNext(c);
                        }
                        return Disposable.Empty;
                    });

            //1
            obs
                .BufferUntil('a', 'b', 200)
                .SelectMany(p => p)
                .Subscribe(list.Add);
            ts.AdvanceTo(str.Length);
            new string(list.ToArray()).Should().Be("aa0000baa1111b");

            //2
            list.Clear();
            obs
                .BufferUntil('4', '3', 200)
                .Retry()
                .SelectMany(p => p)
                .Subscribe(list.Add);
            ts.AdvanceTo(str.Length);
            new string(list.ToArray()).Should().Be("4aa0000bbaa1111bb43");
        }

        [Fact()]
        public void ByteBufferUntilTest()
        {
            var str = Encoding.Default.GetBytes("1234aa01020304bb4321");
            string rst = string.Empty;

            var ts = new TestScheduler();

            var start = Encoding.Default.GetBytes("aa");
            var stop = Encoding.Default.GetBytes("bb");

            var obs = Observable.Create<byte>(x =>
                        {
                            foreach (var c in str)
                            {
                                x.OnNext(c);
                            }
                            return Disposable.Empty;
                        })
                .Buffer(2, 2)
               .Select(p => p.ToArray());

            //1
            obs
               .BufferUntil(start, stop, 200)
               .Select(p => Encoding.Default.GetString(p))
               .Subscribe(p => rst = p);
            ts.AdvanceTo(str.Length);
            rst.Should().Be("aa01020304bb");

            //2
            rst = "";
            stop = Encoding.Default.GetBytes("ff");
            obs
               .BufferUntil(start, stop, 200)
               .Select(p => Encoding.Default.GetString(p))
               .Subscribe(p => rst = p);
            ts.AdvanceTo(str.Length);
            rst.Should().Be("");

            //3
            rst = "";
            stop = Encoding.Default.GetBytes("bb");
            obs
               .BufferUntil(start, stop, 200)
               .Select(p => Encoding.Default.GetString(p))
               .Subscribe(p => rst = p);
            ts.AdvanceTo(str.Length);
            rst.Should().Be("aa01020304bb");

            //4
            rst = "";
            obs
               .BufferUntil(start, 0, 200)
               .Select(p => Encoding.Default.GetString(p))
               .Subscribe(p => rst = p);
            ts.AdvanceTo(str.Length);
            rst.Should().Be("aa");

            //5
            rst = "";
            obs
               .BufferUntil(start, 6, 200)
               .Select(p => Encoding.Default.GetString(p))
               .Subscribe(p => rst = p);
            ts.AdvanceTo(str.Length);
            rst.Should().Be("aa0102");
        }
    }
}