using IBot.Events.Misc;

namespace IBot.Events.CustomEventArgs
{
    internal class RoomStateAllEventArgs
    {
        public RoomStateAllEventArgs(RoomStateTags tags, string channel)
        {
            Tags = tags;
            Channel = channel;
        }

        public RoomStateTags Tags { get; }
        public string Channel { get; }
    }
}