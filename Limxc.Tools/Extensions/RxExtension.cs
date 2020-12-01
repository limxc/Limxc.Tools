using System;
using System.Collections.Generic;
using System.Reactive.Linq;

namespace Limxc.Tools.Extensions
{
    public static class RxExtension
    {
        public static T DisposeWith<T>(this T disposable, ICollection<IDisposable> container)
            where T : IDisposable
        {
            container.Add(disposable);
            return disposable;
        }

        public static IObservable<T> Debug<T>(this IObservable<T> obs, string msg = "")
        {
            return obs.Do(p => 
            {
                System.Diagnostics.Debug.WriteLine($"****** {msg??"Rx"} : {p.ToString()} ******");
            });
        }
    }
}