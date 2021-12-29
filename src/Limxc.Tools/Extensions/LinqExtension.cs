using System;
using System.Collections.Generic;

namespace Limxc.Tools.Extensions
{
    public static class LinqExtension
    {
        public static IEnumerable<IEnumerable<T>> Batch<T>(this IEnumerable<T> source, int size)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (size <= 0) throw new ArgumentOutOfRangeException(nameof(size));

            var batch = new List<T>();
            foreach (var item in source)
            {
                batch.Add(item);
                if (batch.Count == size)
                {
                    yield return batch;
                    batch = new List<T>();
                }
            }

            yield return batch;
        }

        public static IEnumerable<IEnumerable<T>> Window<T>(this IEnumerable<T> source, int size)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (size <= 0) throw new ArgumentOutOfRangeException(nameof(size));

            return _();

            IEnumerable<IEnumerable<T>> _()
            {
                using (var iter = source.GetEnumerator())
                {
                    var window = new T[size];
                    int i;
                    for (i = 0; i < size && iter.MoveNext(); i++)
                        window[i] = iter.Current;

                    if (i < size)
                    {
                        var newWindow = new T[i];
                        Array.Copy(window, 0, newWindow, 0, i);
                        yield return newWindow;
                        yield break;
                    }

                    while (iter.MoveNext())
                    {
                        var newWindow = new T[size];
                        Array.Copy(window, 1, newWindow, 0, size - 1);
                        newWindow[size - 1] = iter.Current;

                        yield return window;
                        window = newWindow;
                    }

                    yield return window;
                }
            }
        }
    }
}