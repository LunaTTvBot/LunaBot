namespace IRCConnectionTest.Events.CustomEventArgs
{
    internal class RoomStateLangModeEventArgs
    {
        public RoomStateLangModeEventArgs(bool langMode, string lang, string channel)
        {
            LangMode = langMode;
            Lang = lang;
            Channel = channel;
        }

        public bool LangMode { get; }
        public string Lang { get; }
        public string Channel { get; }
    }
}