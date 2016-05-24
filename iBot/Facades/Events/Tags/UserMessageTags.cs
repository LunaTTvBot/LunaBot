﻿namespace IBot.Facades.Events.Tags
{
    public class UserMessageTags
    {
        public UserMessageTags(string color, string displayName, string emotes, bool moderator, long roomId,
            bool subscriber,
            bool turbo, long userId, string userType)
        {
            Color = color;
            DisplayName = displayName;
            Emotes = emotes;
            Moderator = moderator;
            RoomId = roomId;
            Subscriber = subscriber;
            Turbo = turbo;
            UserId = userId;
            UserType = userType;
        }

        internal UserMessageTags(IBot.Events.Tags.UserMessageTags tags)
        {
            Color = tags.Color;
            DisplayName = tags.DisplayName;
            Emotes = tags.Emotes;
            Moderator = tags.Moderator;
            RoomId = tags.RoomId;
            Subscriber = tags.Subscriber;
            Turbo = tags.Turbo;
            UserId = tags.UserId;
            UserType = tags.UserType;
        }

        public string Color { get; }
        public string DisplayName { get; }
        public string Emotes { get; }
        public bool Moderator { get; }
        public long RoomId { get; }
        public bool Subscriber { get; }
        public bool Turbo { get; }
        public long UserId { get; }
        public string UserType { get; }
    }
}