namespace IBot.Events.CustomEventArgs
{
    internal class RoomStateSlowModeEventArgs
    {
        public RoomStateSlowModeEventArgs(bool slowMode, int slowTime, string channel)
        {
            SlowMode = slowMode;
            SlowTime = slowTime;
            Channel = channel;
        }

        public bool SlowMode { get; }
        public int SlowTime { get; }
        public string Channel { get; }
    }
}