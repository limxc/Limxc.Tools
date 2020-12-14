using Limxc.Tools.Extensions;
using System;

namespace Limxc.Tools.Pipeline.Context
{
    public class PipeContextSnapshot<T> where T : class
    {
        public PipeContextSnapshot(T body, string desc)
        {
            Body = body.DeepCopy();
            CreateTime = DateTime.Now;
            Desc = desc;
        }

        public T Body { get; }

        public DateTime CreateTime { get; }

        public string Desc { get; }

        public override string ToString()
        {
            return $"{Desc} @ {CreateTime:mm:ss fff} : {Body}";
        }
    }
}