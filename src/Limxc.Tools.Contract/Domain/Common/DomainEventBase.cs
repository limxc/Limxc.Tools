using System;

namespace Limxc.Tools.Contract.Domain.Common
{
    public abstract class BaseDomainEvent
    {
        public DateTime DateOccurred { get; protected set; } = DateTime.UtcNow;
    }
}