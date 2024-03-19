using System.Collections.Generic;

namespace Limxc.Tools.Utils
{
    public static class BindingsExtension
    {
        public static void UnbindWith(this Bindings source, IList<Bindings> bindings)
        {
            bindings.Add(source);
        }

        public static void Unbind(this IList<Bindings> bindings)
        {
            foreach (var binding in bindings)
                binding.Unbind();
        }
    }
}