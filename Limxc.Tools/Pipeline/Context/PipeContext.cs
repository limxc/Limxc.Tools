using System.Collections.Generic;

namespace Limxc.Tools.Pipeline.Context
{
    public class PipeContext<T> where T : class
    {
        public T Body { get; }

        public List<PipeContextSnapshot<T>> Snapshots { get; }

        public PipeContext(T body)
        {
            Body = body;
            Snapshots = new List<PipeContextSnapshot<T>>();
        }

        public void AddSnapshot(string desc)
        {
            var h = new PipeContextSnapshot<T>(Body, desc);
            Snapshots.Add(h);
        }

        public override string ToString()
        {
            return $"{Body} Snapshots: {Snapshots.Count}";
        }
    }
}