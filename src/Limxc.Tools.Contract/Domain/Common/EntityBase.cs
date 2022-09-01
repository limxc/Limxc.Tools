using System;
using System.Collections.Generic;

namespace Limxc.Tools.Contract.Domain.Common
{
    public abstract class BaseEntity<TId>
    {
        private readonly List<BaseDomainEvent> _domainEvents = new List<BaseDomainEvent>();

        public TId Id { get; set; }

        public IReadOnlyCollection<BaseDomainEvent> GetDomainEvents()
        {
            return _domainEvents.AsReadOnly();
        }

        public void AddDomainEvent(BaseDomainEvent baseDomainBaseDomainEvent)
        {
            _domainEvents.Add(baseDomainBaseDomainEvent);
        }

        public void RemoveDomainEvent(BaseDomainEvent baseDomainBaseDomainEvent)
        {
            _domainEvents.Remove(baseDomainBaseDomainEvent);
        }

        public void ClearDomainEvents()
        {
            _domainEvents.Clear();
        }
    }

    public abstract class BaseEntity : BaseEntity<Guid>
    {
    }
}