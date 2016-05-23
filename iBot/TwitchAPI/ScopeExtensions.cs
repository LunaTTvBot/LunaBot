using System.Linq;
using IBot.Core;
using IBot.TwitchAPI.Models;

namespace IBot.TwitchAPI
{
    internal static class ScopeExtensions
    {
        public static string Concat(this Scope scope, string delimiter)
            => string.Join(delimiter, scope.GetFlags()
                                           .Cast<Scope>()
                                           .Where(s => scope.HasFlag(s))
                                           .Select(s => s.GetAttribute<ScopeDefinitionAttribute>().Name));
    }
}
