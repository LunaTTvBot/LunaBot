namespace IBot.Models
{
    internal static class ExtendableExtensions
    {
        public static void Set<T>(this IExtendable extendable, string propertyName, T value) => Extension.Set(extendable, propertyName, value);

        public static T Get<T>(this IExtendable extendable, string propertyName) => Extension.Get<T>(extendable, propertyName);
    }
}
