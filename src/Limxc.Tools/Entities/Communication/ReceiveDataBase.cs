using System;
using Limxc.Tools.Extensions;

namespace Limxc.Tools.Entities.Communication
{
    public abstract class ReceiveDataBase
    {
        protected ReceiveDataBase(DateTime? receiveTime = null)
        {
            ReceiveTime = receiveTime?.ToTimeStamp() ?? DateTime.Now.ToTimeStamp();
        }

        public long ReceiveTime { get; protected set; }

        public string Id { get; protected set; }

        public virtual string Desc()
        {
            return string.Empty;
        }

        public virtual string ToCsv()
        {
            return string.Empty;
        }

        public override string ToString()
        {
            return $"{ReceiveTime.ToDateTime():MM:dd HH:mm:ss fff} | Id:[{Id}] Desc:[{Desc()}] ";
        }
    }
}