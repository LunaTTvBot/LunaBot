using System.Linq;
using IBot.Core;

namespace IBot.Plugins.UserAwards
{
    internal class Award
    {
        public AwardType Type { get; private set; }
        public double Multiplier { get; private set; }
        public double Value { get; private set; }
        public double TotalValue => Multiplier * Value;

        public Award(AwardType type)
        {
            Type = type;
            Multiplier = 1;

            var settings = SettingsManager.GetSettings<AwardSettings>();

            // get a list of all properties for AwardSettings
            // get a list of all attributes for each Property
            // if we found a property with the same AwardType as this one, set Value to its value
            // otherwise continue
            foreach (var property in typeof(AwardSettings).GetProperties())
            {
                foreach (var attribute in property.GetCustomAttributes(false).OfType<AwardTypeAttribute>())
                {
                    if (attribute.Type != type)
                        continue;

                    Value = (double) property.GetValue(settings);
                    return;
                }
            }

            // we only arrive here if we haven't found any Property with
            // [AwardType(type)]
            // in the settings
            Value = 0.0d;
        }

        public void Add(double increment)
        {
            Multiplier += increment;
        }

        public void Remove(double decrement)
        {
            Multiplier -= decrement;
        }
    }
}
