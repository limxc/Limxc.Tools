using Microsoft.Reactive.Testing;
using System;
using System.Reactive;

namespace Limxc.Tools.DeviceComm.Tests
{
    public static class TestSchedulerTools
    {
        public static Recorded<Notification<T>> CreateOnNext<T>(int time, Func<T> value)
        {
            return new Recorded<Notification<T>>(time, Notification.CreateOnNext<T>(value()));
        }

        public static Recorded<Notification<T>> CreateOnError<T>(int time, Func<T> value) where T : Exception
        {
            return new Recorded<Notification<T>>(time, Notification.CreateOnError<T>(value()));
        }

        public static Recorded<Notification<T>> CreateOnComplete<T>(int time)
        {
            return new Recorded<Notification<T>>(time, Notification.CreateOnCompleted<T>());
        }
    }
}