using IBot.Events.Tags;

namespace IBot.Events.Args.RoomState
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