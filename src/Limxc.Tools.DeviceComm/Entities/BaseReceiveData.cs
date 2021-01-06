using Limxc.Tools.Extensions;
using System;

namespace Limxc.Tools.DeviceComm.Entities
{
    public abstract class BaseReceiveData
    {
        protected BaseReceiveData(DateTime? receiveTime = null)
        {
            ReceiveTime = receiveTime.HasValue ? receiveTime.Value.ToTimeStamp() : DateTime.Now.ToTimeStamp();
        }

        public long ReceiveTime { get; protected set; }

        public string Id { get; protected set; }

        public virtual string Desc() => string.Empty;

        public virtual string ToCsv() => string.Empty;

        public override string ToString()
        {
            return $"{ReceiveTime.ToDateTime():MM:dd HH:mm:ss fff} | Id:[{Id}] Desc:[{Desc()}] ";
        }
    }
}