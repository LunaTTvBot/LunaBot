using System;
using System.Collections.Generic;
using System.Linq;

namespace IBot.Core
{
    internal static class EnumExtensions
    {
        public static T GetAttribute<T>(this Enum value)
            where T : Attribute
        {
            var type = value.GetType();
            var name = Enum.GetName(type, value);

            return type.GetField(name)
                       .GetCustomAttributes(false)
                       .OfType<T>()
                       .SingleOrDefault();
        }

        public static IEnumerable<Enum> GetFlags(this Enum value)
        {
            foreach (Enum e in Enum.GetValues(value.GetType()))
                if (value.HasFlag(e))
                    yield return e;
        }
    }
}
