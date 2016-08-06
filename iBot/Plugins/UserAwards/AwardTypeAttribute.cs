using System;

namespace IBot.Plugins.UserAwards
{
    internal class AwardTypeAttribute : Attribute
    {
        public AwardType Type { get; set; }

        public AwardTypeAttribute(AwardType type)
        {
            Type = type;
        }
    }
}
