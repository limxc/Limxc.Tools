using System;
using System.Collections.Generic;

namespace Limxc.Tools.DeviceComm.Extensions
{
    public static class RxExtension
    {
        public static T DisposeWith<T>(this T disposable, ICollection<IDisposable> container)
            where T : IDisposable
        {
            container.Add(disposable);
            return disposable;
        }
    }
}