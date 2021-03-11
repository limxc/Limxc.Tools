using System;
using System.Reactive;
using Microsoft.Reactive.Testing;

namespace Limxc.Tools.DeviceComm.Tests
{
    public static class TestSchedulerTools
    {
        public static Recorded<Notification<T>> CreateOnNext<T>(int time, Func<T> value)
        {
            return new(time, Notification.CreateOnNext(value()));
        }

        public static Recorded<Notification<T>> CreateOnError<T>(int time, Func<T> value) where T : Exception
        {
            return new(time, Notification.CreateOnError<T>(value()));
        }

        public static Recorded<Notification<T>> CreateOnComplete<T>(int time)
        {
            return new(time, Notification.CreateOnCompleted<T>());
        }
    }
}