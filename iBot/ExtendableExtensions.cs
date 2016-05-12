using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IBot
{
    internal static class ExtendableExtensions
    {
        public static void Set<T>(this IExtendable extendable, string propertyName, T value) => Extension.Set(extendable, propertyName, value);

        public static T Get<T>(this IExtendable extendable, string propertyName) => Extension.Get<T>(extendable, propertyName);
    }
}
