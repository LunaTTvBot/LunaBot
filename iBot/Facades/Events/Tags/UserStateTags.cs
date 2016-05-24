namespace IBot.Facades.Events.Tags
{
    public class UserStateTags
    {
        public UserStateTags(string color, string displayName, string emoteSets, bool moderator,
            bool subscriber,
            bool turbo, string userType)
        {
            Color = color;
            DisplayName = displayName;
            EmoteSets = emoteSets;
            Moderator = moderator;
            Subscriber = subscriber;
            Turbo = turbo;
            UserType = userType;
        }

        internal UserStateTags(IBot.Events.Tags.UserStateTags tags)
        {
            Color = tags.Color;
            DisplayName = tags.DisplayName;
            EmoteSets = tags.EmoteSets;
            Moderator = tags.Moderator;
            Subscriber = tags.Subscriber;
            Turbo = tags.Turbo;
            UserType = tags.UserType;
        }

        public string Color { get; }
        public string DisplayName { get; }
        public string EmoteSets { get; }
        public bool Moderator { get; }
        public bool Subscriber { get; }
        public bool Turbo { get; }
        public string UserType { get; }
    }
}