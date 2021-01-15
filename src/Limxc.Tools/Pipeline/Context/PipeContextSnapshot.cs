using System;

namespace Limxc.Tools.Pipeline.Context
{
    public class PipeContextSnapshot<T> where T : class
    {
        public PipeContextSnapshot(T body, string desc)
        {
            Body = body;
            Desc = desc;
            CreateTime = DateTime.Now;
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