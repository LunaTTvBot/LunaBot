namespace IBot.Events.Args.RoomState
{
    internal class RoomStateSubscriberModeEventArgs
    {
        public RoomStateSubscriberModeEventArgs(bool subMode, string channel)
        {
            SubMode = subMode;
            Channel = channel;
        }

        public bool SubMode { get; }
        public string Channel { get; }
    }
}