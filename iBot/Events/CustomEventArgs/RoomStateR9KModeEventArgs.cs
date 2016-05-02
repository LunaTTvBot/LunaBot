namespace IBot.Events.CustomEventArgs
{
    internal class RoomStateR9KModeEventArgs
    {
        public RoomStateR9KModeEventArgs(bool r9KMode, string channel)
        {
            R9KMode = r9KMode;
            Channel = channel;
        }

        public bool R9KMode { get; }
        public string Channel { get; }
    }
}