using System;

// ReSharper disable PartialTypeWithSinglePart

namespace Limxc.Tools.Contract.Domain.Common
{
    public abstract partial class BaseDomainEvent
    {
        public DateTime DateOccurred { get; protected set; } = DateTime.UtcNow;
    }
}