using System;
using System.Collections.Generic;

namespace Limxc.Tools.Pipeline.Context
{
    public class PipeContext<T>
        where T : class
    {
        private readonly Func<T, T> _cloner;

        public PipeContext(T body, Func<T, T> cloner)
        {
            Body = body;
            _cloner = cloner;
            Snapshots = new List<PipeContextSnapshot<T>>();
        }

        public T Body { get; }

        public List<PipeContextSnapshot<T>> Snapshots { get; }

        public void AddSnapshot(string desc)
        {
            var body = _cloner?.Invoke(Body);
            if (body != null)
                Snapshots.Add(new PipeContextSnapshot<T>(body, desc));
        }

        public override string ToString()
        {
            return $"{Body} Snapshots: {Snapshots.Count}";
        }
    }
}
