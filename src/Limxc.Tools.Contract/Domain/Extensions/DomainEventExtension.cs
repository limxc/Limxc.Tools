using System.Linq;
using System.Reflection;
using Limxc.Tools.Contract.Domain.Common;
using Limxc.Tools.Contract.Domain.Interfaces;

namespace Limxc.Tools.Contract.Domain.Extensions
{
    public static class DomainEventExtension
    {
        public static void DispatchDomainEvent<TId>(this BaseEntity<TId> baseEntity, IDomainEventDispatcher dispatcher)
        {
            if (dispatcher == null)
                return;

            foreach (var prop in baseEntity.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance)
                         .ToList())
            {
                var type = prop.PropertyType;

                if (typeof(BaseEntity<TId>).IsAssignableFrom(type))
                {
                    var value = (BaseEntity<TId>)prop.GetValue(baseEntity);
                    dispatcher.DispatchAndClearEvents(value);

                    ((BaseEntity<TId>)prop.GetValue(baseEntity)).DispatchDomainEvent(dispatcher);
                }
            }

            dispatcher.DispatchAndClearEvents(baseEntity);
        }
    }
}