using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IBot.TwitchAPI.Models
{
    internal class ScopeDefinitionAttribute : Attribute
    {
        public string Name { get; private set; }

        public ScopeDefinitionAttribute(string scopeName)
        {
            Name = scopeName;
        }
    }
}
