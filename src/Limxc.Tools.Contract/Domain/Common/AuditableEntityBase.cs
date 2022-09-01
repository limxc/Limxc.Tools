using System;

namespace Limxc.Tools.Contract.Domain.Common
{
    public abstract class AuditableBaseEntity<TId> : BaseEntity<TId>
    {
        public DateTime Created { get; set; }

        public string CreatedBy { get; set; }

        public DateTime? LastModified { get; set; }

        public string LastModifiedBy { get; set; }
    }

    public abstract class AuditableBaseEntity : AuditableBaseEntity<Guid>
    {
    }
}